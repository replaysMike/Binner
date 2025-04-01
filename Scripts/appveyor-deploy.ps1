# =================================
# Binner Appveyor Deployment Script
# =================================
$versionTag = "v$env:APPVEYOR_BUILD_VERSION"

Write-Host "Deploying $env:APPVEYOR_BUILD_VERSION" -ForegroundColor magenta

# build Dockerfile
Write-Host "Building Docker image with tag '$versionTag'..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()
if ($env:BUILDTARGETS.Contains("#docker#")) {
  sed -i -e "s/0.0.0/$env:APPVEYOR_BUILD_VERSION/g" .\Binner\.env
  cat .\Binner\.env
  docker login -u="$env:DOCKER_USERNAME" -p="$env:DOCKER_PASSWORD"
  Set-Location -Path .\Binner
  Write-Host "Building tag binnerofficial/binner:$versionTag"
  docker build --no-cache --progress=plain -t "binnerofficial/binner:$versionTag" .
  Set-Location -Path ..\
}
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }

# build & publish Dockerfile
Write-Host "Publishing Docker image with tag '$versionTag'..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()
  docker login -u=$env:DOCKER_USERNAME -p=$env:DOCKER_PASSWORD
  Set-Location -Path .\Binner
  docker push $env:DOCKER_USERNAME/binner
  Set-Location -Path ..\
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
