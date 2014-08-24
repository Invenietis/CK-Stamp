using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Mono.Cecil;

namespace CK.Releaser
{
    public class PersistentInfo
    {
        public readonly string RepositoryError;
        public readonly bool IsDirty;
        public readonly BasicVersionOnBranch ReleasedTag;
        public readonly string BranchName;
        public readonly string CommitSha;
        public readonly string UserName;

        public PersistentInfo( Repository r )
        {
            UserName = String.IsNullOrWhiteSpace( Environment.UserDomainName )
                           ? Environment.UserName
                           : string.Format( @"{0}\{1}", Environment.UserDomainName, Environment.UserName );
            if( r == null ) RepositoryError = "No repository.";
            else
            {
                var branch = r.Head;
                if( branch.Tip == null ) RepositoryError = "Unitialized repository.";
                else
                {
                    CommitSha = branch.Tip.Sha;
                    var repositoryStatus = r.Index.RetrieveStatus();
                    IsDirty =  repositoryStatus.Added.Any()
                                || repositoryStatus.Missing.Any()
                                || repositoryStatus.Modified.Any()
                                || repositoryStatus.Removed.Any()
                                || repositoryStatus.Staged.Any();
                    ReleasedTag = FindReleasedVersion( r, branch.Tip );
                    if( ReleasedTag.IsValid )
                    {
                        BranchName = ReleasedTag.BranchName;
                    }
                    else
                    {
                        BranchName = branch.Name;
                    }
                }
            }
        }

        public static PersistentInfo LoadFromPath( string path )
        {
            using( var repo = GitDirFinder.TryLoadFromPath( path ) )
            {
                return new PersistentInfo( repo );
            }
        }

        static BasicVersionOnBranch FindReleasedVersion( Repository r, Commit commit )
        {
            foreach( var tag in r.Tags )
            {
                if( tag.Target.Sha == commit.Sha )
                {
                    BasicVersionOnBranch v = BasicVersionOnBranch.TryParse( tag.Name );
                    if( v.IsValid ) return v;
                }
            }
            return new BasicVersionOnBranch();
        }


    }
}
