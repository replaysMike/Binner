$project = ".\Binner\Binner.ReleaseBuild.sln"
$releaseConfiguration = "Release"
$framework = "net7.0"

Write-Host "Building $env:APPVEYOR_BUILD_VERSION" -ForegroundColor magenta

Write-Host "Installing build dependencies..." -ForegroundColor green
choco install -y innosetup
Install-Product node ''

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

Write-Host "Restoring packages..." -ForegroundColor green
dotnet restore $project
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }

Write-Host "Building..." -ForegroundColor green
dotnet build $project -c $releaseConfiguration
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }

# publish specific profiles
Write-Host "Publishing for each environment..." -ForegroundColor green

$buildEnv = "win10-x64"
Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }

Write-Host "Building the UI..." -ForegroundColor cyan
cd .\Binner\Binner.Web\ClientApp
npm install
Write-Host "npm exit code: $LASTEXITCODE"
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }
npm run build
Write-Host "npm exit code: $LASTEXITCODE"
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }
cd ..\..\..\

robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

$buildEnv = "linux-x64"
Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }
robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

$buildEnv = "linux-arm"
Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }
robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

$buildEnv = "linux-arm64"
Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }
robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

$buildEnv = "ubuntu.14.04-x64"
Write-Host "Publishing for $buildEnv.." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }
robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

$buildEnv = "osx.10.12-x64"
Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }
robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

# transform configurations per environment
# windows does not need a transform

$buildEnv = "win10-x64"
Copy-Item -Force -Path .\Binner\scripts\windows\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish

$buildEnv = "linux-x64"
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
if (!$?) { exit -1  }
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
if (!$?) { exit -1  }
Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish

$buildEnv = "linux-arm"
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
if (!$?) { exit -1  }
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
if (!$?) { exit -1  }
Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish

$buildEnv = "linux-arm64"
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
if (!$?) { exit -1  }
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
if (!$?) { exit -1  }
Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish

$buildEnv = "ubuntu.14.04-x64"
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
if (!$?) { exit -1  }
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
if (!$?) { exit -1  }
Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish
if (!$?) { exit -1  }

$buildEnv = "osx.10.12-x64"
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
if (!$?) { exit -1  }
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
if (!$?) { exit -1  }
Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish
if (!$?) { exit -1  }

# build installers
Write-Host "Building Installers" -ForegroundColor green
Set-Location -Path .\Binner\BinnerInstaller
(Get-Content .\BinnerInstaller.iss).replace('MyAppVersion "0.0"', 'MyAppVersion "' + $env:APPVEYOR_BUILD_VERSION + '"') | Set-Content .\BinnerInstaller.iss
Write-Host "Building installer using the following script:" -ForegroundColor cyan
cat .\BinnerInstaller.iss
.\build-installer.cmd
if ($LastExitCode -ne 0) { exit $LASTEXITCODE }


# package artifacts
Write-Host "Creating artifact archives..." -ForegroundColor green
Set-Location -Path ..\..\

# (note) windows doesn't need this, it has it's own installer that won't be archived

tar -czf Binner_linux-x64.targz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\linux-x64\publish .

tar -czf Binner_linux-arm.targz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\linux-arm\publish .

tar -czf Binner_linux-arm64.targz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\linux-arm64\publish .

tar -czf Binner_ubuntu.14.04-x64.targz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\ubuntu.14.04-x64\publish .

tar -czf Binner_osx.10.12-x64.targz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\osx.10.12-x64\publish .

Write-Host "Uploading Artifacts" -ForegroundColor green
Get-ChildItem .\Binner\BinnerInstaller\*.exe | % { Push-AppveyorArtifact $_.FullName }

# rename these artifacts to include the build version number
Get-ChildItem .\*.targz -recurse | % { Push-AppveyorArtifact $_.FullName -FileName "$($_.Basename)-$env:APPVEYOR_BUILD_VERSION.tar.gz" }