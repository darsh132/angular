$ErrorActionPreference = 'Stop'

Write-Host '=== Jira Clone Quality Gate ===' -ForegroundColor Cyan

Write-Host '`n[1/4] Restoring backend...' -ForegroundColor Yellow
dotnet restore backend/JiraClone.Api/JiraClone.Api.csproj

dotnet restore backend/JiraClone.Api.Tests/JiraClone.Api.Tests.csproj

Write-Host '`n[2/4] Building backend...' -ForegroundColor Yellow
dotnet build backend/JiraClone.Api/JiraClone.Api.csproj --no-restore --configuration Release

dotnet build backend/JiraClone.Api.Tests/JiraClone.Api.Tests.csproj --no-restore --configuration Release

Write-Host '`n[3/4] Running backend tests...' -ForegroundColor Yellow
dotnet test backend/JiraClone.Api.Tests/JiraClone.Api.Tests.csproj --no-build --configuration Release

Write-Host '`n[4/4] Building and testing Angular...' -ForegroundColor Yellow
Push-Location frontend
try {
    npm ci
    npm run build
    npm test -- --watch=false --no-progress
}
finally {
    Pop-Location
}

Write-Host '`nQUALITY GATE PASSED' -ForegroundColor Green
