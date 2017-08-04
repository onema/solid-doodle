#!/usr/bin/env bash

dotnet restore
dotnet build
dotnet lambda package --configuration release --framework netcoreapp1.0 --output-package bin/release/netcoreapp1.0/deploy-package.zip --verbose
serverless  deploy function --function LogParser -v $@