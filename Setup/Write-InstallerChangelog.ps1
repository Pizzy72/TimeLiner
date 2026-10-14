# Generate installer release notes from the same source and formatter as the application.
param(
    [string] $OutputPath = (Join-Path $PSScriptRoot '..\artifacts\installer\CHANGELOG.txt')
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path $PSScriptRoot
$Markdown = [IO.File]::ReadAllText((Join-Path $RepositoryRoot 'CHANGELOG.md'))
$Releases = [regex]::Matches($Markdown, '(?m)^##\s+\[([^\]]+)\][^\r\n]*')
if ($Releases.Count -eq 0) {
    throw 'CHANGELOG.md does not contain a release section.'
}
$Start = $Releases[0].Index
$ReleaseHistory = $Markdown.Substring($Start).TrimEnd()

if (-not ('InstallerChangelogFormatter' -as [type])) {
    $FormatterSource = [IO.File]::ReadAllText(
        (Join-Path $RepositoryRoot 'Source/TimeLiner/Common/ChangelogText.cs'))
    # Expose the internal application formatter only inside this build process.
    Add-Type -TypeDefinition ($FormatterSource + @"
public static class InstallerChangelogFormatter
{
    public static string Convert(string markdown) =>
        TimeLiner.Common.ChangelogText.Convert(markdown);
}
"@)
}
$Text = [InstallerChangelogFormatter]::Convert($ReleaseHistory)
$FullOutputPath = [IO.Path]::GetFullPath($OutputPath)
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($FullOutputPath)) | Out-Null
[IO.File]::WriteAllText($FullOutputPath, $Text, [Text.UTF8Encoding]::new($true))