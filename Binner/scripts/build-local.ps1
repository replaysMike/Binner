$project = "..\Binner.ReleaseBuild.sln"
$releaseConfiguration = "Release"
$framework = "net9.0"

Write-Host "Building..." -ForegroundColor magenta
dotnet build $project -c $releaseConfiguration

$buildEnv = "win-x64"
Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained --property WarningLevel=0
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }

Write-Host "Building the UI..." -ForegroundColor cyan
cd ..\Binner.Web\ClientApp
npm install
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
npm run build
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
cd ..\..\
robocopy .\Binner.Web\ClientApp\build .\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

cd .\scripts
$buildEnv = "linux-x64"
Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained --property WarningLevel=0
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
robocopy .\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

$buildEnv = "win-x64"
Copy-Item -Force -Path .\scripts\windows\* -Destination .\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish

$buildEnv = "linux-x64"
Move-Item -Force -Path .\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
if (!$?) { exit -1 }
Move-Item -Force -Path .\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
if (!$?) { exit -1 }
Copy-Item -Force -Path .\scripts\unix\* -Destination .\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish

Write-Host "Building Installers" -ForegroundColor green
Set-Location -Path .\BinnerInstaller
Write-Host "Building installer using the following script:" -ForegroundColor cyan
.\build-installer.cmd
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }

Write-Host "Creating artifact archives..." -ForegroundColor green
Set-Location -Path ..\

tar -czf Binner_linux-x64.targz -C .\Binner.Web\bin\$($releaseConfiguration)\$framework\linux-x64\publish .
# move files to dist folder
Move-Item -Force -Path 