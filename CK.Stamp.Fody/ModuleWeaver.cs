using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CK.Releaser;
using LibGit2Sharp;
using Mono.Cecil;
using Mono.Collections.Generic;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public string SolutionDirectoryPath { get; set; }
    public string AddinDirectoryPath { get; set; }
    public string AssemblyFilePath { get; set; }
    
    static bool _isPathSet;
    StandardInfo _info;
    string _informationalVersion;

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
    }

    public void Execute()
    {
        SetSearchPath();
        using( var repo = GitDirFinder.TryLoadFromPath( SolutionDirectoryPath ) )
        {
            _info = new StandardInfo( new PersistentInfo( repo ), ModuleDefinition );

            var customAttributes = ModuleDefinition.Assembly.CustomAttributes;
            var attr = customAttributes.FirstOrDefault(x => x.AttributeType.Name == "AssemblyInformationalVersionAttribute");
            if( attr != null )
            {
                _informationalVersion = (string)attr.ConstructorArguments[0].Value;
                _informationalVersion = _info.ReplaceTokens( _informationalVersion );
                attr.ConstructorArguments[0] = new CustomAttributeArgument( ModuleDefinition.TypeSystem.String, _informationalVersion );
            }
            else
            {
                var versionAttribute = GetVersionAttribute();
                var constructor = ModuleDefinition.Import( versionAttribute.Methods.First( x => x.IsConstructor ) );
                attr = new CustomAttribute( constructor );
                _informationalVersion = _info.ToString();
                attr.ConstructorArguments.Add( new CustomAttributeArgument( ModuleDefinition.TypeSystem.String, _informationalVersion ) );
                customAttributes.Add( attr );
            }
        }
    }

    void SetSearchPath()
    {
        if( _isPathSet ) return;
        _isPathSet = true;
        var nativeBinaries = Path.Combine( AddinDirectoryPath, "NativeBinaries", GetProcessorArchitecture() );
        var existingPath = Environment.GetEnvironmentVariable( "PATH" );
        var newPath = string.Concat( nativeBinaries, Path.PathSeparator, existingPath );
        Environment.SetEnvironmentVariable( "PATH", newPath );
    }

    static string GetProcessorArchitecture()
    {
        return Environment.Is64BitProcess ? "amd64" : "x86";
    }

    TypeDefinition GetVersionAttribute()
    {
        var msCoreLib = ModuleDefinition.AssemblyResolver.Resolve( "mscorlib" );
        var msCoreAttribute = msCoreLib.MainModule.Types.FirstOrDefault( x => x.Name == "AssemblyInformationalVersionAttribute" );
        if( msCoreAttribute != null )
        {
            return msCoreAttribute;
        }
        var systemRuntime = ModuleDefinition.AssemblyResolver.Resolve( "System.Runtime" );
        return systemRuntime.MainModule.Types.First( x => x.Name == "AssemblyInformationalVersionAttribute" );
    }

    public void AfterWeaving()
    {
        var verPatchPath = Path.Combine( AddinDirectoryPath, "verpatch.exe" );
        // Product version MUST start with the binary version number.
        var productVersion = _info.AssemblyVersion.ToString() + " - " + _informationalVersion;
        var arguments = string.Format( "\"{0}\" /pv \"{1}\" /high /va {2}", AssemblyFilePath, productVersion, _info.AssemblyVersion.ToString() );
        LogInfo( string.Format( "Patching version using: {0} {1}", verPatchPath, arguments ) );
        var startInfo = new ProcessStartInfo
        {
            FileName = verPatchPath,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetTempPath()
        };
        using( var process = Process.Start( startInfo ) )
        {
            if( !process.WaitForExit( 1000 ) )
            {
                var timeoutMessage = string.Format( "Failed to apply product version to Win32 resources in 1 second.\r\nFailed command: {0} {1}", verPatchPath, arguments );
                throw new WeavingException( timeoutMessage );
            }

            if( process.ExitCode == 0 )
            {
                return;
            }
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            var message = string.Format( "Failed to apply product version to Win32 resources.\r\nOutput: {0}\r\nError: {1}", output, error );
            throw new WeavingException( message );
        }
    }
}