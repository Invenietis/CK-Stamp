using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CK.Releaser;
using Mono.Cecil;

namespace CK.Stamp.Fody.Tests
{
    public class ProcessAssemblyBaseTest
    {
        protected readonly Assembly AfterAssembly;
        protected readonly string BeforeAssemblyPath;
        protected readonly string AfterAssemblyPath;
        protected readonly PersistentInfo PersistentInfo;
        protected readonly StandardInfo StandardInfo;


        protected ProcessAssemblyBaseTest( string relativePath )
        {
            BeforeAssemblyPath = Path.GetFullPath( relativePath );
#if !DEBUG
            BeforeAssemblyPath = BeforeAssemblyPath.Replace("Debug", "Release");
#endif

            AfterAssemblyPath = BeforeAssemblyPath.Replace( ".dll", "2.dll" );
            File.Copy( BeforeAssemblyPath, AfterAssemblyPath, true );

            var moduleDefinition = ModuleDefinition.ReadModule( AfterAssemblyPath );

            var currentDirectory = AssemblyLocation.CurrentDirectory();
            PersistentInfo = PersistentInfo.LoadFromPath( currentDirectory );
            StandardInfo = new StandardInfo( PersistentInfo, moduleDefinition );

            var moduleWeaver = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AddinDirectoryPath = currentDirectory,
                SolutionDirectoryPath = currentDirectory,
                AssemblyFilePath = AfterAssemblyPath,
            };

            moduleWeaver.Execute();
            moduleDefinition.Write( AfterAssemblyPath );
            moduleWeaver.AfterWeaving();

            AfterAssembly = Assembly.LoadFile( AfterAssemblyPath );

        }
    }
}
