# Manual Database Initialization Script
# Run this if automatic initialization fails

Write-Host "Creating database manually..." -ForegroundColor Cyan

$dbPath = "bin\Debug\net8.0-windows\Data\HealthManagement.db"
$sqlPath = "client_mgmt_schema.sql"

# Ensure Data directory exists
New-Item -ItemType Directory -Path "bin\Debug\net8.0-windows\Data" -Force | Out-Null

# Check if we have sqlite3
try {
    sqlite3 --version | Out-Null
    $hasSqlite = $true
} catch {
    $hasSqlite = $false
}

if (-not $hasSqlite) {
    Write-Host "SQLite3 not found. Downloading..."  -ForegroundColor Yellow
    $url = "https://sqlite.org/2024/sqlite-tools-win-x64-3460100.zip"
    $zipPath = "sqlite-tools.zip"
    Invoke-WebRequest -Uri $url -OutFile $zipPath
    Expand-Archive -Path $zipPath -DestinationPath "." -Force
    Remove-Item $zipPath
    $env:PATH += ";$PWD\sqlite-tools-win-x64-3460100"
}

Write-Host "Initializing database from SQL schema..." -ForegroundColor Cyan
Get-Content $sqlPath | sqlite3 $dbPath

$dbInfo = Get-Item $dbPath
Write-Host "`nDatabase created successfully!" -ForegroundColor Green
Write-Host "Path: $dbPath"
Write-Host "Size: $($dbInfo.Length) bytes"

Write-Host "`nVerifying tables..."  -ForegroundColor Cyan
sqlite3 $dbPath ".tables"

Write-Host "`nDone! You can now run the application." -ForegroundColor Green
