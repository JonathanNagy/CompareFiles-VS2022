NewtonSoft.Json.dll:
VSIX doesn't appear to pick up NuGet packages automatically. Simpliest fix was to add it as 
an asset, which puts a copy in the project root folder.
Check source.extension.vsix.manifest under Assets tab to see entry. In future versions, the
file will need to be replaced by whatever version is pulled from nuget.