using System;
using System.IO;
using System.Linq;
using CK.Releaser;
using LibGit2Sharp;
using Mono.Cecil;
using NUnit.Framework;

namespace CK.Stamp.Fody.Tests
{
    [TestFixture]
    public class TokenResolverTests
    {
        ModuleDefinition _moduleDefinition;
        PersistentInfo _persistentInfo;
        StandardInfo _standardInfo;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var beforeAssemblyPath = Path.GetFullPath( @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll" );
#if (!DEBUG)
            beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif
            _moduleDefinition = ModuleDefinition.ReadModule( beforeAssemblyPath );
            _persistentInfo = PersistentInfo.LoadFromPath( Environment.CurrentDirectory );
            _standardInfo = new StandardInfo( _persistentInfo, _moduleDefinition );
        }

        [Test]
        public void Replace_version()
        {
            var result = _standardInfo.ReplaceTokens( "%version%" );
            Assert.AreEqual( "10.20.30.40", result );
        }

        [Test]
        public void Replace_version1()
        {
            var result = _standardInfo.ReplaceTokens( "%version1%" );
            Assert.AreEqual( "10", result );
        }

        [Test]
        public void Replace_version2()
        {
            var result = _standardInfo.ReplaceTokens( "%version2%" );
            Assert.AreEqual( "10.20", result );
        }

        [Test]
        public void Replace_version3()
        {
            var result = _standardInfo.ReplaceTokens( "%version3%" );
            Assert.AreEqual( "10.20.30", result );
        }

        [Test]
        public void Replace_version4()
        {
            var result = _standardInfo.ReplaceTokens( "%version4%" );
            Assert.AreEqual( "10.20.30.40", result );
        }

        [Test]
        public void Replace_branch()
        {
            var result = _standardInfo.ReplaceTokens( "%branch%" );
            Assert.AreEqual( _persistentInfo.BranchName, result );
        }

        [Test]
        public void Replace_githash()
        {
            var result = _standardInfo.ReplaceTokens( "%githash%" );
            Assert.AreEqual( _persistentInfo.CommitSha, result );
        }

        [Test]
        public void Replace_haschanges()
        {
            var result = _standardInfo.ReplaceTokens( "%haschanges%" );
            Assert.AreEqual( _persistentInfo.IsDirty ? "HasChanges" : String.Empty, result );
        }

        [Test]
        public void Replace_user()
        {
            var result = _standardInfo.ReplaceTokens( "%user%" );
            Assert.AreEqual( _persistentInfo.UserName, result );
        }

        [Test]
        public void Replace_machine()
        {
            var result = _standardInfo.ReplaceTokens( "%machine%" );
            Assert.AreEqual( Environment.MachineName, result );
        }

        [Test]
        public void Replace_environment_variables()
        {
            var environmentVariables = Environment.GetEnvironmentVariables();
            var expected = string.Join( "--", environmentVariables.Values.Cast<string>() );

            var replacementTokens = string.Join( "--", environmentVariables.Keys.Cast<string>()
                                                                            .Select( key => "%env[" + key + "]%" )
                                                                            .ToArray() );
            var result = _standardInfo.ReplaceTokens( replacementTokens );

            Assert.AreEqual( expected, result );
        }

        [Test]
        public void Replace_CK_Standard()
        {
            var result = _standardInfo.ReplaceTokens( "%ck-standard%" );
            Assert.AreEqual( _standardInfo.ToString(), result );
        }
    }
}