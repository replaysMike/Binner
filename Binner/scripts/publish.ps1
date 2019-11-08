# Publishing of releases
dotnet publish -c Release -f netcoreapp3.0 --self-contained --runtime win-x64
dotnet publish -c Release -f netcoreapp3.0 --self-contained --runtime linux-x64
dotnet publish -c Release -f netcoreapp3.0 --self-contained --runtime osx.10.12-x64