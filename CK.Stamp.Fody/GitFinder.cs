using System;
using System.IO;
using LibGit2Sharp;

namespace CK.Releaser
{
    public class GitFinder
    {
        public static string TreeWalkForGitDir( string currentDirectory )
        {
            while( true )
            {
                var gitDir = Path.Combine( currentDirectory, @".git" );
                if( Directory.Exists( gitDir ) )
                {
                    return gitDir;
                }
                var parent = Directory.GetParent( currentDirectory );
                if( parent == null )
                {
                    break;
                }
                currentDirectory = parent.FullName;
            }
            return null;
        }

        public static Repository LoadFromPath( string path )
        {
            try
            {
                path = Path.GetFullPath( path );
                var gitDir = GitFinder.TreeWalkForGitDir( path );
                return new Repository( gitDir );
            }
            catch( Exception exception )
            {
                if( exception.Message.Contains( "LibGit2Sharp.Core.NativeMethods" ) || exception.Message.Contains( "FilePathMarshaler" ) )
                {
                    throw new WeavingException( "Restart of Visual Studio required due to update of 'CK.Stamp.Fody': " + exception.Message );
                }
                throw;
            }
        }
    }
}
