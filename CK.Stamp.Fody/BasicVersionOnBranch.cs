using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CK.Releaser
{
    /// <summary>
    /// Captures Major.Minor.Patch-BranchName pattern.
    /// This is NOT a semantic version, this is the version associated to a commit 
    /// in the repository.
    /// </summary>
    public struct BasicVersionOnBranch
    {
        /// <summary>
        /// When <see cref="IsValid"/> is true, necessarily greater or equal to 0.
        /// </summary>
        public readonly int Major;
        /// <summary>
        /// When <see cref="IsValid"/> is true, necessarily greater or equal to 0.
        /// </summary>
        public readonly int Minor;
        /// <summary>
        /// When <see cref="IsValid"/> is true, necessarily greater or equal to 0.
        /// </summary>
        public readonly int Patch;
        /// <summary>
        /// When <see cref="IsValid"/> is true, necessarily not null, nor empty nor be full of white spaces.
        /// </summary>
        public readonly string BranchName;

        /// <summary>
        /// Gets whether this <see cref="VersionOnBranch"/> is valid.
        /// </summary>
        public bool IsValid
        {
            get { return BranchName != null; }
        }

        BasicVersionOnBranch( int major, int minor, int patch, string branchName )
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            BranchName = branchName;
        }

        public bool Equals( Version other )
        {
            return Major == other.Major && Minor == other.Minor && Patch == other.Build;
        }

        /// <summary>
        /// Gets the string version (without 'v' prefix) and without <see cref="BranchName"/>.
        /// </summary>
        /// <returns>The Major.Minor.Patch.</returns>
        public string ToStringWithoutBranchName()
        {
            return String.Format( CultureInfo.InvariantCulture, "{0}.{1}.{2}", Major, Minor, Patch );
        }

        /// <summary>
        /// Gets the string version (without 'v' prefix).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format( CultureInfo.InvariantCulture, "{0}.{1}.{2}-{3}", Major, Minor, Patch, BranchName );
        }

        static Regex _regex = new Regex( @"^v?(?<1>0|[1-9][0-9]*)\.(?<2>0|[1-9][0-9]*)\.(?<3>0|[1-9][0-9]*)-(?<4>\w+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant );

        /// <summary>
        /// Attempts to parse a string like "4.0.0-master" or "v1.0-5-develop". The branch must be composed of at least one (non space) char.
        /// Numbers can not start with a 0 (except if it is 0).
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <param name="v">Resulting version.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool TryParse( string s, out BasicVersionOnBranch v )
        {
            v = TryParse( s );
            return v.IsValid;
        }

        /// <summary>
        /// Parses a string like "4.0.0-master" or "v1.0-5-develop". The branch must be composed of at least one (non space) char.
        /// Numbers can not start with a 0 (except if it is 0).
        /// Returns a VersionOnBranch where <see cref="VersionOnBranch.IsValid"/> is false if the string is not valid.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <returns>Resulting version (can be invalid).</returns>
        public static BasicVersionOnBranch TryParse( string s )
        {
            Match m = _regex.Match( s );
            if( m.Success )
            {
                return new BasicVersionOnBranch( Int32.Parse( m.Groups[1].Value ), Int32.Parse( m.Groups[2].Value ), Int32.Parse( m.Groups[3].Value ), m.Groups[4].Value );
            }
            return new BasicVersionOnBranch();
        }

    }
}
