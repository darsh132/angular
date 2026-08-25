#!/usr/bin/env bash
set -euo pipefail

echo '=== Jira Clone Quality Gate ==='

echo '[1/4] Restoring backend...'
dotnet restore backend/JiraClone.Api/JiraClone.Api.csproj
dotnet restore backend/JiraClone.Api.Tests/JiraClone.Api.Tests.csproj

echo '[2/4] Building backend...'
dotnet build backend/JiraClone.Api/JiraClone.Api.csproj --no-restore --configuration Release
dotnet build backend/JiraClone.Api.Tests/JiraClone.Api.Tests.csproj --no-restore --configuration Release

echo '[3/4] Running backend tests...'
dotnet test backend/JiraClone.Api.Tests/JiraClone.Api.Tests.csproj --no-build --configuration Release

echo '[4/4] Building and testing Angular...'
cd frontend
npm ci
npm run build
npm test -- --watch=false --no-progress

echo 'QUALITY GATE PASSED'
