using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Releaser;
using NUnit.Framework;

namespace CK.Stamp.Fody.Tests
{
    [TestFixture]
    public class ReleaseTagVersionTests
    {
        [TestCase( "v0.0.0-master" )]
        [TestCase( "v3.0.1-develop" )]
        [TestCase( "v3.0.1-B" )]
        [TestCase( "v3.0.1-B-fix.1" )]
        [TestCase( "v3.0.14-develop-fix.1045" )]
        [TestCase( "v3.0.14-master" )]
        [TestCase( "v3.0.14-master-fix.1" )]
        public void parsing_valid_released_tags( string tag )
        {
            Assert.That( ReleaseTagVersion.TryParse( tag ).IsValid );
            Assert.That( tag.ToString(), Is.EqualTo( tag ) );
        }

        [TestCase( "v3.0.1" )]
        [TestCase( "3.0.1" )]
        [TestCase( "v3.0.1-" )]
        [TestCase( "v3.0.1-1" )]
        [TestCase( "v3.0.1-1A" )]
        [TestCase( "v3.0.1-B.C" )]
        [TestCase( "v3.0.14-develop-fix.0" )]
        [TestCase( "v3.0.14-master-fix" )]
        [TestCase( "v3.0.14-A-B-fix.2" )]
        [TestCase( "v3.0.14-fix.1" )]
        [TestCase( "3.0-master" )]
        [TestCase( "3-master" )]
        public void parsing_invalid_released_tags( string tag )
        {
            Assert.That( ReleaseTagVersion.TryParse( tag ).IsValid, Is.False );
        }
    }
}
