# intended as a tool to perform local release builds instead of relying on the AppVeyor pipeline (faster)
# notes: Only handling the Windows & Linux build targets
$project = ".\Binner\Binner.ReleaseBuild.sln"
$releaseConfiguration = "Release"
$framework = "net9.0"

Write-Host "Installing build dependencies..." -ForegroundColor green
# chocolatey required: Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
choco install -y innosetup

Write-Host "Building local $releaseConfiguration build..." -ForegroundColor magenta
dotnet build $project -c $releaseConfiguration --property WarningLevel=0

$buildEnv = "win-x64"
Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained --property WarningLevel=0
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }

Write-Host "Building the UI..." -ForegroundColor cyan
cd .\Binner\Binner.Web\ClientApp
npm install
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
npm run build
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
cd ..\..\..\
robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

$buildEnv = "linux-x64"
Write-Host "Publishing for $buildEnv..." -ForegroundColor cyan
dotnet publish $project -r $buildEnv -c $releaseConfiguration --self-contained --property WarningLevel=0
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }
robocopy .\Binner\Binner.Web\ClientApp\build .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\ClientApp\build /s

$buildEnv = "win-x64"
Copy-Item -Force -Path .\Binner\scripts\windows\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish

$buildEnv = "linux-x64"
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.Unix.Production.json -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\appsettings.json
if (!$?) { exit -1 }
Move-Item -Force -Path .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.Unix.config -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish\nlog.config
if (!$?) { exit -1 }
Copy-Item -Force -Path .\Binner\scripts\unix\* -Destination .\Binner\Binner.Web\bin\$releaseConfiguration\$framework\$buildEnv\publish

Write-Host "Building Installers" -ForegroundColor green
Set-Location -Path .\Binner\BinnerInstaller
#(Get-Content .\BinnerInstaller.iss).replace('MyAppVersion "0.0"', 'MyAppVersion "' + (($env:APPVEYOR_BUILD_VERSION).split('-')[0]) + '"') | Set-Content .\BinnerInstaller.iss
(Get-Content .\BinnerInstaller.iss).replace('@NETFRAMEWORK@', "$framework") | Set-Content .\BinnerInstaller.iss
Write-Host "Building installer using the following script:" -ForegroundColor cyan
.\build-installer.cmd
Move-Item -Force -Path .\BinnerSetup-winx64-0.0.exe -Destination ..\..\
Set-Location -Path ..\..\
if ($LastExitCode -ne 0) { Write-Host "Exiting - error code '$LastExitCode'"; exit $LastExitCode }

Write-Host "Creating artifact archives..." -ForegroundColor green

tar -czf Binner_linux-x64.tar.gz -C .\Binner\Binner.Web\bin\$($releaseConfiguration)\$framework\linux-x64\publish .
# move files to dist folder
#Move-Item -Force -Path 