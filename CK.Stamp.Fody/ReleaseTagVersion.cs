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
    /// Captures Major.Minor.Patch-BranchName[-fix.FixNumber] pattern.
    /// This is NOT a semantic version, this is the version associated to a commit 
    /// in the repository.
    /// </summary>
    public struct ReleaseTagVersion
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
        /// True when it is a "-fix.XXX" tag where XXX is a number greater that 1.
        /// </summary>
        public bool IsFix { get { return FixNumber > 0; } }

        /// <summary>
        /// Fix number. Greater than 0 when the tag ends with "-fix.[FixNumber]".
        /// A suffix like "-fix.0" is invalid.
        /// </summary>
        public readonly int FixNumber;

        /// <summary>
        /// Gets whether this <see cref="VersionOnBranch"/> is valid.
        /// </summary>
        public bool IsValid
        {
            get { return BranchName != null; }
        }

        ReleaseTagVersion( int major, int minor, int patch, string branchName, int fixNumber )
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            BranchName = branchName;
            FixNumber = fixNumber;
        }

        public bool Equals( Version other )
        {
            return Major == other.Major && Minor == other.Minor && Patch == other.Build && FixNumber == other.Revision;
        }

        /// <summary>
        /// Gets the string version with the 'v' prefix and with "-fix.[FixNumber]" if this <see cref="IsFix"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format( CultureInfo.InvariantCulture, "v{0}.{1}.{2}-{3}{4}", Major, Minor, Patch, BranchName, IsFix ? "-fix." + FixNumber.ToString() : String.Empty );
        }

        static Regex _regex = new Regex( @"^v(?<1>0|[1-9][0-9]*)\.(?<2>0|[1-9][0-9]*)\.(?<3>0|[1-9][0-9]*)-(?<4>[a-zA-Z]\w*)(\-fix\.(?<5>[1-9][0-9]*))?$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        /// <summary>
        /// Attempts to parse a string like "v4.0.0-master" or "v1.0-5-develop-fix.14".
        /// Initial 'v' is required.
        /// Numbers can not start with a 0 (except if it is 0).
        /// The branch must start with a leter and followed by any number of a-z, A-Z, 0-9, including the _ ([a-zA-Z]\w* regular expression).
        /// The optional "-fix." must be followed by an integer greater than 0. 
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <param name="v">Resulting version.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool TryParse( string s, out ReleaseTagVersion v )
        {
            v = TryParse( s );
            return v.IsValid;
        }

        /// <summary>
        /// Attempts to parse a string like "v4.0.0-master" or "v1.0-5-develop-fix.14".
        /// Initial 'v' is required.
        /// Numbers can not start with a 0 (except if it is 0).
        /// The branch must start with a leter and followed by any number of a-z, A-Z, 0-9, including the _ ([a-zA-Z]\w* regular expression).
        /// The optional "-fix." must be followed by an integer greater than 0. 
        /// Returns a ReleaseTagVersion where <see cref="ReleaseTagVersion.IsValid"/> is false if the string is not valid.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <returns>Resulting version (can be invalid).</returns>
        public static ReleaseTagVersion TryParse( string s )
        {
            Match m = _regex.Match( s );
            if( m.Success )
            {
                string sFix = m.Groups[5].Value;
                int fixNumber = 0;
                if( sFix.Length == 0 || Int32.TryParse( sFix, out fixNumber ) )
                {
                    int major, minor, patch;
                    if( Int32.TryParse( m.Groups[1].Value, out major ) 
                        && Int32.TryParse( m.Groups[2].Value, out minor ) 
                        && Int32.TryParse( m.Groups[3].Value, out patch ) )
                    {
                        return new ReleaseTagVersion( Int32.Parse( m.Groups[1].Value ), Int32.Parse( m.Groups[2].Value ), Int32.Parse( m.Groups[3].Value ), m.Groups[4].Value, fixNumber );
                    }
                }
            }
            return new ReleaseTagVersion();
        }

    }
}
