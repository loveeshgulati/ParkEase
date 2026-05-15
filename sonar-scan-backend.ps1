# =========================================================
# sonar-scan-backend.ps1
# Run SonarQube analysis on ParkEase .NET Backend
#
# USAGE:
#   .\sonar-scan-backend.ps1 -Token "your_sonar_token"
#   .\sonar-scan-backend.ps1  (uses $env:SONAR_TOKEN if set)
# =========================================================

param(
    [string]$Token = $env:SONAR_TOKEN,
    [string]$SonarUrl = "http://localhost:9000",
    [string]$ProjectKey = "parkease-backend"
)

# --- Validate token ---
if (-not $Token) {
    Write-Error "No SonarQube token provided. Pass -Token 'xxxx' or set `$env:SONAR_TOKEN"
    exit 1
}

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  ParkEase Backend - SonarQube Analysis" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  Server  : $SonarUrl" -ForegroundColor Gray
Write-Host "  Project : $ProjectKey" -ForegroundColor Gray
Write-Host ""

# --- Check dotnet-sonarscanner is installed ---
if (-not (Get-Command "dotnet-sonarscanner" -ErrorAction SilentlyContinue)) {
    Write-Host "[!] dotnet-sonarscanner not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-sonarscanner
}

# --- Clean previous coverage output ---
if (Test-Path "coverage") {
    Remove-Item -Recurse -Force "coverage"
    Write-Host "[1/4] Cleaned previous coverage output" -ForegroundColor Green
}
New-Item -ItemType Directory -Force -Path "coverage" | Out-Null

# --- Step 1: Begin SonarQube analysis ---
Write-Host ""
Write-Host "[2/4] Starting SonarQube scanner..." -ForegroundColor Green
dotnet sonarscanner begin `
    /k:"$ProjectKey" `
    /d:sonar.host.url="$SonarUrl" `
    /d:sonar.token="$Token" `
    /d:sonar.cs.opencover.reportsPaths="coverage\coverage.opencover.xml" `
    /d:sonar.exclusions="**/obj/**,**/bin/**,**/Migrations/**,**/*.Designer.cs" `
    /d:sonar.coverage.exclusions="**/Migrations/**,**/*Program.cs"

if ($LASTEXITCODE -ne 0) { Write-Error "SonarScanner begin failed"; exit 1 }

# --- Step 2: Build the solution ---
Write-Host ""
Write-Host "[3/4] Building solution..." -ForegroundColor Green
dotnet build ParkEase.sln --no-incremental -c Release

if ($LASTEXITCODE -ne 0) { Write-Error "Build failed"; exit 1 }

# --- Step 3: Run tests with coverage ---
Write-Host ""
Write-Host "[4/4] Running tests with code coverage..." -ForegroundColor Green
dotnet test ParkEase.Tests/ParkEase.Tests.csproj `
    --no-build `
    --configuration Release `
    --collect:"XPlat Code Coverage" `
    --results-directory ./coverage `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Some tests failed, but continuing with analysis..."
}

# Copy coverage file to expected flat path
$coverageFile = Get-ChildItem -Path "coverage" -Recurse -Filter "coverage.opencover.xml" | Select-Object -First 1
if ($coverageFile) {
    Copy-Item -Path $coverageFile.FullName -Destination "coverage\coverage.opencover.xml" -Force
    Write-Host "Coverage report found: $($coverageFile.FullName)" -ForegroundColor Green
} else {
    Write-Warning "No coverage.opencover.xml found. Coverage metrics won't be available."
}

# --- Step 4: Upload results to SonarQube ---
Write-Host ""
Write-Host "[5/4] Uploading results to SonarQube..." -ForegroundColor Green
dotnet sonarscanner end /d:sonar.token="$Token"

if ($LASTEXITCODE -ne 0) { Write-Error "SonarScanner end failed"; exit 1 }

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  Analysis complete!" -ForegroundColor Green
Write-Host "  View results: $SonarUrl/dashboard?id=$ProjectKey" -ForegroundColor Yellow
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
