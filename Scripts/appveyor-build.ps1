$project = ".\Binner\Binner.ReleaseBuild.sln"
$releaseConfiguration = "Release"
$framework = "net9.0"

Write-Host "Building $env:APPVEYOR_BUILD_VERSION" -ForegroundColor magenta
Write-Host "Build Targets: $env:BUILDTARGETS" -ForegroundColor magenta

$sw = [Diagnostics.Stopwatch]::StartNew()
  Write-Host "Installing build dependencies..." -ForegroundColor green
  dotnet tool install --global dotnet-ef --version 9.0.2
  Update-NodeJsInstallation 22.14.0 x64
  choco install -y innosetup
  npm install -g npm@latest

  Write-Host "Checking versions..." -ForegroundColor green
  Write-Host "Node" -ForegroundColor cyan
  node --version
  Write-Host "Npm" -ForegroundColor cyan
  npm --version
  Write-Host "Dotnet" -ForegroundColor cyan
  dotnet --version
  Write-Host "Tar" -ForegroundColor cyan
  tar --version

  Write-Host "Configuring environment..." -ForegroundColor green
  $env:NODE_OPTIONS="--max_old_space_size=16384"
  Write-Host "NODE_OPTIONS=$env:NODE_OPTIONS" -ForegroundColor cyan
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }

Write-Host "Restoring packages..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()
  dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
  dotnet restore -s https://api.nuget.org/v3/index.json $project
  #dotnet workload update
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }

Write-Host "Building..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()
  dotnet build $project -c $releaseConfiguration --property WarningLevel=0
  if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }

# publish specific profiles
Write-Host "Publishing for each environment..." -ForegroundColor green

$buildEnv = "win-x64"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
  $sw = [Diagnostics.Stopwatch]::StartNew()
    dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained --property WarningLevel=0
    if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
  $sw.Stop()
  $sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
}

Write-Host "Building the UI..." -ForegroundColor cyan
$sw = [Diagnostics.Stopwatch]::StartNew()
  cd .\Binner\Binner.Web\ClientApp
  npm install
  Write-Host "npm exit code: $LASTEXITCODE"
  if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
  npm run build
  Write-Host "npm exit code: $LASTEXITCODE"
  if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
  cd ..\..\..\

  robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }

$buildEnv = "linux-x64"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
  $sw = [Diagnostics.Stopwatch]::StartNew()
    dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained --property WarningLevel=0
    if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
    robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s
  $sw.Stop()
  $sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
}

$buildEnv = "linux-arm"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
  $sw = [Diagnostics.Stopwatch]::StartNew()
    dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained --property WarningLevel=0
    if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
    robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s
  $sw.Stop()
  $sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
}

$buildEnv = "linux-arm64"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
  $sw = [Diagnostics.Stopwatch]::StartNew()
    dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained --property WarningLevel=0
    if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
    robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s
  $sw.Stop()
  $sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
}

$buildEnv = "osx-x64"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
  $sw = [Diagnostics.Stopwatch]::StartNew()
    dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained --property WarningLevel=0
    if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
    robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s
  $sw.Stop()
  $sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
}

# transform configurations per environment
# windows does not need a transform

$buildEnv = "win-x64"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Copy-Item -Force -Path .\Binner\scripts\windows\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish
}

$buildEnv = "linux-x64"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
  if (!$?) { exit -1  }
  Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
  if (!$?) { exit -1  }
  Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish
}

$buildEnv = "linux-arm"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
  if (!$?) { exit -1  }
  Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
  if (!$?) { exit -1  }
  Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish
}

$buildEnv = "linux-arm64"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
  if (!$?) { exit -1  }
  Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
  if (!$?) { exit -1  }
  Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish
}

$buildEnv = "osx-x64"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
  if (!$?) { exit -1  }
  Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
  if (!$?) { exit -1  }
  Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish
  if (!$?) { exit -1  }
}

# build installers
$buildEnv = "win-x64"
if ($env:BUILDTARGETS.Contains("#$buildEnv#")) {
  Write-Host "Building Installers" -ForegroundColor green
  $sw = [Diagnostics.Stopwatch]::StartNew()
    Set-Location -Path .\Binner\BinnerInstaller
    (Get-Content .\BinnerInstaller.iss).replace('MyAppVersion "0.0"', 'MyAppVersion "' + (($env:APPVEYOR_BUILD_VERSION).split('-')[0]) + '"') | Set-Content .\BinnerInstaller.iss
    (Get-Content .\BinnerInstaller.iss).replace('@NETFRAMEWORK@', "$framework") | Set-Content .\BinnerInstaller.iss
    Write-Host "Building installer using the following script:" -ForegroundColor cyan
    cat .\BinnerInstaller.iss
    .\build-installer.cmd
    if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
    Set-Location -Path ..\..\
  $sw.Stop()
  $sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
}

# package artifacts
Write-Host "Creating artifact archives..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()

  # (note) windows doesn't need this, it has it's own installer that won't be archived
  if ($env:BUILDTARGETS.Contains("#linux-x64#")) {
    tar -czf Binner_linux-x64.targz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\linux-x64\publish .
  }
  if ($env:BUILDTARGETS.Contains("#linux-arm#")) {
    tar -czf Binner_linux-arm.targz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\linux-arm\publish .
  }
  if ($env:BUILDTARGETS.Contains("#linux-arm64#")) {
    tar -czf Binner_linux-arm64.targz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\linux-arm64\publish .
  }
  if ($env:BUILDTARGETS.Contains("#osx-x64#")) {
    tar -czf Binner_osx-x64.targz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\osx-x64\publish .
  }
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }

$sw = [Diagnostics.Stopwatch]::StartNew()
  Write-Host "Uploading Artifacts" -ForegroundColor green
  if ($env:BUILDTARGETS.Contains("#win-x64#")) {
    Get-ChildItem .\Binner\BinnerInstaller\*.exe | % { Push-AppveyorArtifact $_.FullName }
  }

  # rename these artifacts to include the build version number
  Get-ChildItem .\*.targz -recurse | % { Push-AppveyorArtifact $_.FullName -FileName "$($_.Basename)-$env:APPVEYOR_BUILD_VERSION.tar.gz" }
  $sw.Stop()
  $sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }