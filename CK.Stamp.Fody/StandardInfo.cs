using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Mono.Cecil;

namespace CK.Releaser
{
    public class StandardInfo
    {
        public readonly PersistentInfo Persistent;
        public readonly Version AssemblyVersion;

        public bool IsValidRelease
        {
            get 
            {
                return Persistent.RepositoryError == null
                         && Persistent.IsDirty == false
                         && Persistent.ReleasedTag.IsValid
                         && Persistent.ReleasedTag.Equals( AssemblyVersion ); 
            }
        }

        public string InvalidReleaseReason
        {
            get 
            {
                if( Persistent.RepositoryError != null ) return Persistent.RepositoryError;
                if( Persistent.IsDirty ) return "Modified files.";
                if( !Persistent.ReleasedTag.IsValid ) return "Missing or invalid released tag. Must be like 'v0.0.0-branch' or 'v12.34.56-master-fix.1'.";
                if( !Persistent.ReleasedTag.Equals( AssemblyVersion ) ) return String.Format( "Assembly version '{0}' differ from tag '{1}'.", AssemblyVersion.ToString(), Persistent.ReleasedTag.ToString() );
                return null;
            }
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            if( IsValidRelease )
            {
                b.Append( Persistent.ReleasedTag.ToString() ).Append( " - " ).Append( Persistent.UserName ).Append( " - Sha: " ).Append( Persistent.CommitSha );
            }
            else
            {
                b.Append( '!' ).Append( InvalidReleaseReason ).Append( ' ' );
                if( Persistent.BranchName != null ) b.Append( "Branch: " ).Append( Persistent.BranchName );
                b.Append( " - " ).Append( Persistent.UserName );
                if( Persistent.CommitSha != null ) b.Append( " - Sha: " ).Append( Persistent.CommitSha );
            }
            return b.ToString();
        }

        public string ReplaceTokens( string template )
        {
            template = template.Replace( "%ck-standard%", ToString() );

            template = template.Replace( "%version%", AssemblyVersion.ToString() );
            template = template.Replace( "%version1%", AssemblyVersion.ToString( 1 ) );
            template = template.Replace( "%version2%", AssemblyVersion.ToString( 2 ) );
            template = template.Replace( "%version3%", AssemblyVersion.ToString( 3 ) );
            template = template.Replace( "%version4%", AssemblyVersion.ToString( 4 ) );

            template = template.Replace( "%githash%", Persistent.CommitSha );

            template = template.Replace( "%branch%", Persistent.BranchName );

            template = template.Replace( "%haschanges%", Persistent.IsDirty ? "HasChanges" : String.Empty );

            template = template.Replace( "%user%", Persistent.UserName );
            template = template.Replace( "%machine%", Environment.MachineName );

            template = _environmentToken.Replace( template, match => Environment.GetEnvironmentVariable( match.Groups[1].Value ) );

            return template.Trim();
        }

        static Regex _environmentToken = new Regex( @"%env\[([^\]]+)]%" );

        public StandardInfo( PersistentInfo p, Version assemblyVersion )
        {
            Persistent = p;
            AssemblyVersion = assemblyVersion;
        }

        public StandardInfo( PersistentInfo p, ModuleDefinition m )
            : this( p, m.Assembly.Name.Version )
        {
        }

    }
}
