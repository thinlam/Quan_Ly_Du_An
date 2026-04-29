@echo off
setlocal enabledelayedexpansion

REM ============================================
REM ef.bat - Unified EF Core Migration Tool (QLDA)
REM ============================================
REM
REM Usage: ef.bat <Module> <Command> [MigrationName] [Options]
REM   Module: QLDA (required)
REM   Command: add, remove, update, list (required)
REM   MigrationName: Required for 'add' command
REM   Options:
REM     --sqlite   Use SQLite provider (update only)
REM     --force    Force remove without checking
REM
REM Examples:
REM   ef.bat QLDA add AddUserTable          Add migration (SQL Server)
REM   ef.bat QLDA remove                    Remove last migration
REM   ef.bat QLDA remove 0                  Remove ALL migrations
REM   ef.bat QLDA update                    Update SQL Server via dotnet ef
REM   ef.bat QLDA update --sqlite           Create SQLite DB via Migrator
REM   ef.bat QLDA list                      List all migrations
REM ============================================

set MODULE=%1
set COMMAND=%2
set MIGRATION_NAME=%3
set PROVIDER=sqlserver

REM === Parse options ===
for %%a in (%*) do (
    if /i "%%a"=="--sqlite" set PROVIDER=sqlite
    if /i "%%a"=="--sqlserver" set PROVIDER=sqlserver
    if /i "%%a"=="--force" set FORCE_FLAG=--force
)

REM === Clear MIGRATION_NAME if it's a flag ===
if /i "%MIGRATION_NAME%"=="--sqlite" set MIGRATION_NAME=
if /i "%MIGRATION_NAME%"=="--sqlserver" set MIGRATION_NAME=
if /i "%MIGRATION_NAME%"=="--force" set MIGRATION_NAME=

REM === Set ASPNETCORE_ENVIRONMENT ===
if not defined ASPNETCORE_ENVIRONMENT set ASPNETCORE_ENVIRONMENT=Development

REM === Validate Module ===
if "%MODULE%"=="" (
    echo.
    echo ERROR: Module parameter is required
    echo.
    echo Usage: ef.bat ^<Module^> ^<Command^> [MigrationName] [Options]
    echo   Module: QLDA
    echo   Command: add, remove, update, list
    echo.
    pause
    exit /b 1
)

REM === Set Module Paths ===
if /i "%MODULE%"=="QLDA" (
    set MODULE=QLDA
    set MIGRATOR_PATH=QLDA.Migrator\QLDA.Migrator.csproj
    set PERSISTENCE_PATH=QLDA.Persistence\QLDA.Persistence.csproj
    goto :module_found
)

echo.
echo ERROR: Invalid module "%MODULE%"
echo Valid modules: QLDA
echo.
pause
exit /b 1

:module_found

REM === Validate Command ===
if "%COMMAND%"=="" (
    echo.
    echo ERROR: Command parameter is required
    echo.
    echo Usage: ef.bat %MODULE% ^<Command^> [MigrationName] [Options]
    echo   Command: add, remove, update, list
    echo.
    pause
    exit /b 1
)

REM === Display Info ===
echo.
echo ============================================
echo   %MODULE% EF CORE MIGRATION
echo ============================================
echo   Command: %COMMAND%
if not "%MIGRATION_NAME%"=="" echo   Migration: %MIGRATION_NAME%
if /i "%COMMAND%"=="update" echo   Provider: %PROVIDER%
echo ============================================
echo.

REM === Execute Command ===
if /i "%COMMAND%"=="add" goto :add_migration
if /i "%COMMAND%"=="remove" goto :remove_migration
if /i "%COMMAND%"=="update" goto :update_database
if /i "%COMMAND%"=="list" goto :list_migrations

echo.
echo ERROR: Invalid command "%COMMAND%"
echo Valid commands: add, remove, update, list
echo.
pause
exit /b 1

REM ============================================
REM  ADD MIGRATION
REM  Always generates migration files (SQL Server)
REM ============================================
:add_migration
if "%MIGRATION_NAME%"=="" (
    echo ERROR: Migration name required for 'add' command
    echo Usage: ef.bat %MODULE% add ^<MigrationName^>
    pause
    exit /b 1
)
echo Adding migration: %MIGRATION_NAME%
dotnet ef migrations add %MIGRATION_NAME% --project %MIGRATOR_PATH% --startup-project %MIGRATOR_PATH% --context AppDbContext
goto :end

REM ============================================
REM  REMOVE MIGRATION
REM  Removes last (or ALL) migration file(s)
REM ============================================
:remove_migration
if /i not "%MIGRATION_NAME%"=="0" goto :single_remove

echo Removing ALL migrations...
echo.
set REMOVE_COUNT=0

:remove_loop
dotnet ef migrations remove --project %MIGRATOR_PATH% --startup-project %MIGRATOR_PATH% --context AppDbContext --force 2>nul
if %ERRORLEVEL% EQU 0 (
    set /a REMOVE_COUNT+=1
    echo Removed migration #!REMOVE_COUNT!
    goto :remove_loop
)
echo.
echo ============================================
echo   Removed !REMOVE_COUNT! migration(s)
echo ============================================
if !REMOVE_COUNT! GTR 0 exit /b 0
goto :end

:single_remove
echo Removing last migration...
dotnet ef migrations remove --project %MIGRATOR_PATH% --startup-project %MIGRATOR_PATH% --context AppDbContext
goto :end

REM ============================================
REM  UPDATE DATABASE
REM  SQL Server → dotnet ef database update (migration files)
REM  SQLite    → dotnet run Migrator (EnsureCreated)
REM ============================================
:update_database
if /i "%PROVIDER%"=="sqlite" (
    echo Creating SQLite database via %MODULE%.Migrator...
    dotnet run --project %MIGRATOR_PATH% -- --provider sqlite
) else (
    echo Updating SQL Server database via dotnet ef...
    dotnet ef database update --project %MIGRATOR_PATH% --startup-project %MIGRATOR_PATH% --context AppDbContext
)
goto :end

REM ============================================
REM  LIST MIGRATIONS
REM ============================================
:list_migrations
echo Listing migrations...
dotnet ef migrations list --project %MIGRATOR_PATH% --startup-project %MIGRATOR_PATH% --context AppDbContext
goto :end

REM ============================================
REM  RESULT
REM ============================================
:end
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================
    echo   OPERATION COMPLETED SUCCESSFULLY
    echo ============================================
) else (
    echo.
    echo ============================================
    echo   OPERATION FAILED
    echo ============================================
    echo   Error code: %ERRORLEVEL%
)
pause
