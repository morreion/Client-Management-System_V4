# Simple database initializer using the project's SQLite DLL
param(
    [string]$ProjectDir = "C:\Users\admin\source\repos\Client-Management-System_V4"
)

Write-Host "`n=== Database Initializer ===" -ForegroundColor Cyan

# Paths
$binDir = Join-Path $ProjectDir "bin\Debug\net8.0-windows"
$dbPath = Join-Path $binDir "Data\HealthManagement.db"
$sqlPath = Join-Path $ProjectDir "client_mgmt_schema.sql"
$sqliteDll = Join-Path $binDir "System.Data.SQLite.dll"

Write-Host "Project: $ProjectDir"
Write-Host "SQL Schema:  $sqlPath"
Write-Host "Database: $dbPath"
Write-Host "SQLite DLL: $sqliteDll`n"

# Check files
if (-not (Test-Path $sqlPath)) {
    Write-Host "ERROR: SQL schema file not found!" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $sqliteDll)) {
    Write-Host "ERROR: System.Data.SQLite.dll not found! Please build the project first." -ForegroundColor Red
    exit 1
}

# Load SQLite assembly
[System.Reflection.Assembly]::LoadFrom($sqliteDll) | Out-Null

# Create Data directory
New-Item -ItemType Directory -Path (Split-Path $dbPath) -Force | Out-Null

# Delete existing database
if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "Deleted existing database" -ForegroundColor Yellow
}

# Create connection
$connectionString = "Data Source=$dbPath;Version=3;"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)

try {
    $connection.Open()
    Write-Host "Database connection opened`n" -ForegroundColor Green

    # Read and execute SQL
    $schema = Get-Content $sqlPath -Raw
    $statements = $schema -split ';'
    
    $executed = 0
    $failed = 0

    foreach ($statement in $statements) {
        $trimmed = $statement.Trim()
        
        if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('--')) {
            continue
        }

        try {
            $command = $connection.CreateCommand()
            $command.CommandText = $trimmed
            $command.ExecuteNonQuery() | Out-Null
            $executed++

            if ($trimmed -match '^CREATE TABLE.*?"(\w+)"') {
                Write-Host "✓ Created table: $($Matches[1])" -ForegroundColor Green
            }
            elseif ($trimmed.StartsWith('INSERT', [StringComparison]::OrdinalIgnoreCase)) {
                Write-Host "✓ Inserted sample data" -ForegroundColor Gray
            }
            elseif ($trimmed.StartsWith('CREATE INDEX', [StringComparison]::OrdinalIgnoreCase)) {
                Write-Host "✓ Created index" -ForegroundColor Gray
            }
        }
        catch {
            $failed++
            Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    Write-Host "`n=== Summary ===" -ForegroundColor Cyan
    Write-Host "Executed: $executed statements"
    Write-Host "Failed: $failed statements"

    $dbInfo = Get-Item $dbPath
    Write-Host "Database size: $($dbInfo.Length) bytes" -ForegroundColor Cyan

    if ($dbInfo.Length -gt 0) {
        Write-Host "`n✅ SUCCESS! Database initialized!" -ForegroundColor Green
    }
    else {
        Write-Host "`n⚠️  WARNING: Database file is 0 bytes!" -ForegroundColor Yellow
    }
}
finally {
    $connection.Close()
    $connection.Dispose()
}
