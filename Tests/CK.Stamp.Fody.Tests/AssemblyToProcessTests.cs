using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CK.Releaser;
using LibGit2Sharp;
using Mono.Cecil;
using NUnit.Framework;

namespace CK.Stamp.Fody.Tests
{
    [TestFixture]
    public class AssemblyToProcessTests : ProcessAssemblyBaseTest
    {
        public AssemblyToProcessTests()
            : base( @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll" )
        {
        }


        [Test]
        public void EnsureAttributeExists()
        {
            var customAttributes = (AssemblyInformationalVersionAttribute)AfterAssembly.GetCustomAttributes( typeof( AssemblyInformationalVersionAttribute ), false ).First();
            Assert.IsNotNullOrEmpty( customAttributes.InformationalVersion );
            Debug.WriteLine( customAttributes.InformationalVersion );
        }

        [Test]
        public void Win32Resource()
        {
            var productVersion = FileVersionInfo.GetVersionInfo( AfterAssemblyPath ).ProductVersion;
            Assert.That( productVersion, Is.StringStarting( StandardInfo.AssemblyVersion.ToString() ) );
            Assert.That( productVersion, Is.StringContaining( StandardInfo.ToString() ) );
        }

        [Test]
        public void TemplateIsReplaced()
        {
            var customAttributes = (AssemblyInformationalVersionAttribute)AfterAssembly.GetCustomAttributes( typeof( AssemblyInformationalVersionAttribute ), false ).First();
            Assert.That( customAttributes.InformationalVersion, Is.EqualTo( StandardInfo.ToString() ) );
        }


        #if(DEBUG)
        [Test]
        public void PeVerify()
        {
            Verifier.Verify( BeforeAssemblyPath, AfterAssemblyPath );
        }
        #endif

    }
}