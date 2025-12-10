@echo off
echo === Manual Database Initializer ===
echo.
cd /d "C:\Users\admin\source\repos\Client-Management-System_V4"

set DB_PATH=bin\Debug\net8.0-windows\Data\HealthManagement.db
set SQL_PATH=client_mgmt_schema.sql

echo SQL Schema: %SQL_PATH%
echo Database: %DB_PATH%
echo.

if not exist "%SQL_PATH%" (
    echo ERROR: SQL schema file not found!
    pause
    exit /b 1
)

rem Create Data directory
if not exist "bin\Debug\net8.0-windows\Data" mkdir "bin\Debug\net8.0-windows\Data"

rem Delete existing database
if exist "%DB_PATH%" (
    del "%DB_PATH%"
    echo Deleted existing database
)

echo.
echo Please run this command in PowerShell to initialize:
echo.
echo $dll='bin\Debug\net8.0-windows\System.Data.SQLite.dll'; [Reflection.Assembly]::LoadFrom($dll)^|Out-Null; $cn=New-Object System.Data.SQLite.SQLiteConnection('Data Source=bin\Debug\net8.0-windows\Data\HealthManagement.db'); $cn.Open(); Get-Content client_mgmt_schema.sql -Raw -split ';'^|ForEach{if($_.Trim()){$cmd=$cn.CreateCommand();$cmd.CommandText=$_;$cmd.ExecuteNonQuery()}}; $cn.Close()
echo.
pause
