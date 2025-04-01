# =================================
# Binner Appveyor Deployment Script
# =================================
$versionTag = "v$env:APPVEYOR_BUILD_VERSION"

Write-Host "Deploying $env:APPVEYOR_BUILD_VERSION" -ForegroundColor magenta

# build & publish Dockerfile
Write-Host "Publishing Docker image with tag '$versionTag'..." -ForegroundColor green
$sw = [Diagnostics.Stopwatch]::StartNew()
  docker login -u=$env:DOCKER_USERNAME -p=$env:DOCKER_PASSWORD
  docker push $env:DOCKER_USERNAME/binner
$sw.Stop()
$sw.Elapsed | Select-Object @{n = "Elapsed"; e = { $_.Minutes, "m ", $_.Seconds, "s ", $_.Milliseconds, "ms " -join "" } }
