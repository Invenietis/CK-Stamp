using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CK.Releaser;
using LibGit2Sharp;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class ExistingTests
{
    readonly Assembly _assembly;
    readonly string _beforeAssemblyPath;
    readonly string _afterAssemblyPath;
    readonly PersistentInfo _persistentInfo;
    readonly StandardInfo _standardInfo;

    public ExistingTests()
    {
        _beforeAssemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcessExistingAttribute\bin\Debug\AssemblyToProcessExistingAttribute.dll");
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif

        _afterAssemblyPath = _beforeAssemblyPath.Replace(".dll", "2.dll");
        File.Copy(_beforeAssemblyPath, _afterAssemblyPath, true);

        var moduleDefinition = ModuleDefinition.ReadModule(_afterAssemblyPath);

        var currentDirectory = AssemblyLocation.CurrentDirectory();
        _persistentInfo = PersistentInfo.LoadFromPath( currentDirectory );
        _standardInfo = new StandardInfo( _persistentInfo, moduleDefinition );

        var moduleWeaver = new ModuleWeaver
                           {
                               ModuleDefinition = moduleDefinition,
                               AddinDirectoryPath = currentDirectory,
                               SolutionDirectoryPath = currentDirectory,
                               AssemblyFilePath = _afterAssemblyPath,
                           };

        moduleWeaver.Execute();
        moduleDefinition.Write(_afterAssemblyPath);
        moduleWeaver.AfterWeaving();

        _assembly = Assembly.LoadFile(_afterAssemblyPath);
    }


    [Test]
    public void EnsureAttributeExists()
    {
        var customAttributes = (AssemblyInformationalVersionAttribute)_assembly.GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false).First();
        Assert.IsNotNullOrEmpty(customAttributes.InformationalVersion);
        Debug.WriteLine( customAttributes.InformationalVersion );
    }

    [Test]
    public void Win32Resource()
    {
        var productVersion = FileVersionInfo.GetVersionInfo( _afterAssemblyPath ).ProductVersion;
        Assert.That( productVersion, Is.EqualTo( _standardInfo.ToString() ) );
    }


    [Test]
    public void TemplateIsReplaced()
    {
        var customAttributes = (AssemblyInformationalVersionAttribute)_assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).First();
        Assert.That( customAttributes.InformationalVersion, Is.EqualTo( _standardInfo.ToString() ) );
    }


#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(_beforeAssemblyPath, _afterAssemblyPath);
    }
#endif

}