$Platform = "x64"
$BuildMode = "Release"
$RuntimeIdentifier = "win-x64"
$PublishPath = "artifacts\publish"
$SourceZipPath = "artifacts\TimeLinerSource.zip"

# Synopsis: Build TimeLiner.
task Build {
    exec { dotnet clean "TimeLiner.sln" --configuration $BuildMode --property:Platform=$Platform }
    exec { dotnet build "TimeLiner.sln" --configuration $BuildMode --property:Platform=$Platform }
}

# Synopsis: Build and run TimeLiner.
task Run Build, {
    exec {
        dotnet run --project "Source\TimeLiner\TimeLiner.csproj" `
            --configuration $BuildMode `
            --no-build `
            --no-launch-profile `
            --property:Platform=$Platform
    }
}

# Synopsis: Run TimeLiner unit tests.
task Test Build, {
    exec { dotnet test "TimeLiner.sln" --configuration $BuildMode --property:Platform=$Platform --no-build --logger "console;verbosity=detailed" }
}

# Synopsis: Publish a self-contained TimeLiner distribution.
task Publish Build, {
    remove "$PublishPath\*"
    exec {
        dotnet publish "Source\TimeLiner\TimeLiner.csproj" `
            --configuration $BuildMode `
            --runtime $RuntimeIdentifier `
            --self-contained true `
            --output $PublishPath `
            --property:Platform=$Platform
    }
}

# Synopsis: Build TimeLiner installer.
task Pack Publish, {
    remove "Setup\Output\*"
    exec { dotnet iscc "Setup\TimeLiner.iss" }
}

# Synopsis: Build and launch the TimeLiner installer.
task Install Pack, {
    $InstallerPath = Get-ChildItem "Setup\Output\TimeLiner_*.exe" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $InstallerPath)
    {
        throw "Installer was not created."
    }

    exec { Start-Process -FilePath $InstallerPath.FullName -Wait }
}

# Synopsis: Export the checked-out TimeLiner sources as a Zip file.
task Export {
    New-Item -ItemType Directory -Force (Split-Path $SourceZipPath) | Out-Null
    exec { git archive --format zip --output $SourceZipPath HEAD }
}

# Synopsis: Release TimeLiner.
task Release Test,Pack

task . Build
