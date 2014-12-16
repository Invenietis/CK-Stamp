![Icon](https://raw.github.com/Fody/Stamp/master/Icons/package_icon.png)

### This is an add-in for [Fody](https://github.com/Fody/Fody/) adapted from https://github.com/Fody/Stamp.

It stamps an assembly with git data: the repository status, commit point and release tag are used to generate
a detailed AssemblyInformationalVersionAttribute. 

### Nuget package

Available here http://nuget.org/packages/CK.Stamp.Fody 

To Install from the Nuget Package Manager Console 
    
    PM> Install-Package CK.Stamp.Fody

## What it does 

Analyses the environment:
- Extracts the user name (domain\username)
- Extracts the git information:
  - Whether the repository is valid (error can be "No repository." or "Unitialized repository.").
  - Whether some files are dirty (uncomitted changes) or not.
  - The commit Sha1.
  - The branch name and its associated release tag: "v1.2.3-alpha" for instance - currently the format must exactly be like this. It will be relaxed in a next release.
  - The version number is read from the compiled module: the AssemblyVersionAttribute must exist (otherwise it is 0.0.0.0).

Checks if this is a valid release:
- There must be no repository errors.
- There must be no uncommitted files.
- A release tag must exist and its version must be equal to Major.Minor.Build of the Version attribute.

It then sets or creates a `AssemblyInformationalVersionAttribute` if it does not exist:
- If it exists, its content is replaced: see the token list below.
- If it does not exist, it is created as if it was defined with %ck-standard% token:
  -  An invalid release starts with a ! and the error, followed by the branch name, the user name and the Sha1: 
     !Modified files. Branch: f-release3.0 - SPICANDIVE\Olivier - Sha: 814aed234e9e97a0a271e444905e18c0c5e07a1c
  - A valid release starts with the released tags (minus the 'v'):
     3.0.0-alpha - SPICANDIVE\Olivier - Sha: 814aed234e9e97a0a271e444905e18c0c5e07a1c

## Templating the version

You can customize the string used in the `AssemblyInformationalVersionAttribute` by adding some tokens to the string, which Ck.Stamp.Fody will replace.

For example, if you add `[assembly: AssemblyInformationalVersion("%version% Sha=%githash%")]` then Stamp will change it to `[assembly: AssemblyInformationalVersion("3.0.0.0 Sha=814aed234e9e97a0a271e444905e18c0c5e07a1c")]`

The tokens are:
- `%ck-standard%` see above
- `%version%` is replaced with the version (1.0.0.0)
- `%version1%` is replaced with the major version only (1)
- `%version2%` is replaced with the major and minor version (1.0)
- `%version3%` is replaced with the major, minor, and patch version (1.0.0)
- `%githash%` is replaced with the SHA1 hash of the branch tip of the repository
- `%branch%` is replaced with the branch name of the repository
- `%haschanges%` is replaced with the string "HasChanges" if the repository is dirty, else a blank string
- `%semver%` is replaced with a simple semantic version: major, minor and revision version if the branch if master, otherwise it appends a dash and the branch name.

> NOTE: if you already have an AssemblyInformationalVersion attribute and it doesn't use replacement tokens, it will not be modified at all.

## Icon

<a href="http://thenounproject.com/noun/stamp/#icon-No8787" target="_blank">Stamp</a> designed by <a href="http://thenounproject.com/rohithdezinr" target="_blank">Rohith M S</a> from The Noun Project
