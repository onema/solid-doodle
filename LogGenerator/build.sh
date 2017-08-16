#!/usr/bin/env bash

dotnet restore
dotnet build --configuration Release
dotnet publish --configuration Release -r osx.10.11-x64
dotnet publish --configuration Release -r win10-x64
chmod +x bin/Release/netcoreapp1.1/osx.10.11-x64/publish/LogGenerator
echo "delete bin/Debug/"
rm -rf bin/Debug/
echo "delete bin/Release/netcoreapp1.1/win7-x64/"
rm -rf bin/Release/netcoreapp1.1/win7-x64/

