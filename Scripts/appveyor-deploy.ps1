# =================================
# Binner Appveyor Deployment Script
# =================================
$versionTag = "v$env:APPVEYOR_BUILD_VERSION"

Write-Host "Checking versions..." -ForegroundColor green
Write-Host "Docker" -ForegroundColor cyan
docker --version

#Write-Host "Switching to linux docker containers"
#Switch-DockerLinux

Write-Host "Deploying docker image for $env:APPVEYOR_BUILD_VERSION" -ForegroundColor magenta

Set-Location -Path .\Binner

Write-Host "Logging into docker using $env:DOCKER_USERNAME"
docker login -u="$env:DOCKER_USERNAME" -p="$env:DOCKER_PASSWORD"

# build Dockerfile
Write-Host "Building Docker image with tag '$versionTag'..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()
  sed -i -e "s/0.0.0/$env:APPVEYOR_BUILD_VERSION/g" .env
  cat .env
  Write-Host "Building tag binnerofficial/binner:$versionTag"
  docker build --no-cache --progress=plain -t "$env:DOCKER_USERNAME/binner:$versionTag" .
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }

# publish Dockerfile
Write-Host "Publishing Docker image with tag '$versionTag'..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()
  docker push "$env:DOCKER_USERNAME/binner:$versionTag"
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
