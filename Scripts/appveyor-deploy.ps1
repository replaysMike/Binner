# =================================
# Binner Appveyor Deployment Script
# =================================
$versionTag = "v$env:APPVEYOR_BUILD_VERSION"

Write-Host "Checking versions..." -ForegroundColor green
Write-Host "Docker" -ForegroundColor cyan
docker --version

Write-Host "Deploying $env:APPVEYOR_BUILD_VERSION" -ForegroundColor magenta

Set-Location -Path .\Binner

docker login -u="$env:DOCKER_USERNAME" -p="$env:DOCKER_PASSWORD"

# build Dockerfile
Write-Host "Building Docker image with tag '$versionTag'..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()
  sed -i -e "s/0.0.0/$env:APPVEYOR_BUILD_VERSION/g" .\Binner\.env
  cat .env
  Write-Host "Building tag binnerofficial/binner:$versionTag"
  docker build --no-cache --progress=plain -t "binnerofficial/binner:$versionTag" .
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }

# publish Dockerfile
Write-Host "Publishing Docker image with tag '$versionTag'..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()
  docker push $env:DOCKER_USERNAME/binner
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
