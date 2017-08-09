#!/usr/bin/env bash

dotnet restore ../LogParser.csproj
dotnet build ../LogParser.csproj
(cd ../ ; dotnet lambda package ../LogParser.csproj --configuration release --framework netcoreapp1.0 --output-package bin/release/netcoreapp1.0/deploy-package.zip --verbose)
serverless deploy -v $@