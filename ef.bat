@echo off
setlocal enabledelayedexpansion

REM ============================================
REM ef.bat - Unified EF Core Migration Tool (QLDA)
REM ============================================
REM
REM Usage: ef.bat <Command> [MigrationName] [Options]
REM   Command: add, remove, update, list, apply (required)
REM   MigrationName: Required for 'add' command
REM   Options:
REM     --provider sqlite|sqlserver  Target provider (default: sqlserver)
REM     --force                      Force remove without checking
REM
REM Examples:
REM   ef.bat add AddUserTable              Add migration (SQL Server)
REM   ef.bat add AddUserTable --force      Add migration (ignore warnings)
REM   ef.bat remove                        Remove last migration
REM   ef.bat remove 0                      Remove ALL migrations
REM   ef.bat update                        Update SQL Server via Migrator
REM   ef.bat update --provider sqlite      Create SQLite DB via Migrator
REM   ef.bat list                          List all migrations
REM   ef.bat apply                         Same as update (alias)
REM ============================================

set COMMAND=%1
set MIGRATION_NAME=%2
set PROVIDER=sqlserver
set FORCE_FLAG=

REM === Parse options ===
for %%a in (%*) do (
    if /i "%%a"=="--provider" set _NEXT_PROVIDER=true
    if defined _NEXT_PROVIDER (
        set PROVIDER=%%a
        set _NEXT_PROVIDER=
    )
    if /i "%%a"=="--force" set FORCE_FLAG=--force
    if /i "%%a"=="--sqlite" set PROVIDER=sqlite
    if /i "%%a"=="--sqlserver" set PROVIDER=sqlserver
)

REM === Clear MIGRATION_NAME if it's a flag ===
if /i "%MIGRATION_NAME%"=="--provider" set MIGRATION_NAME=
if /i "%MIGRATION_NAME%"=="--force" set MIGRATION_NAME=
if /i "%MIGRATION_NAME%"=="--sqlite" set MIGRATION_NAME=
if /i "%MIGRATION_NAME%"=="--sqlserver" set MIGRATION_NAME=

REM === Project paths ===
set MIGRATOR_PATH=QLDA.Migrator\QLDA.Migrator.csproj
set PERSISTENCE_PATH=QLDA.Persistence\QLDA.Persistence.csproj

REM === Validate Command ===
if "%COMMAND%"=="" (
    echo.
    echo ERROR: Command parameter is required
    echo.
    echo Usage: ef.bat ^<Command^> [MigrationName] [Options]
    echo   Commands: add, remove, update, list
    echo   Options: --provider sqlite^|sqlserver, --force
    echo.
    echo Examples:
    echo   ef.bat add AddUserTable
    echo   ef.bat remove
    echo   ef.bat update --provider sqlite
    echo   ef.bat list
    echo.
    pause
    exit /b 1
)

REM === Display Info ===
echo.
echo ============================================
echo   QLDA EF CORE MIGRATION
echo ============================================
echo   Command: %COMMAND%
if not "%MIGRATION_NAME%"=="" echo   Migration: %MIGRATION_NAME%
echo   Provider: %PROVIDER%
echo ============================================
echo.

REM === Execute Command ===
if /i "%COMMAND%"=="add" goto :add_migration
if /i "%COMMAND%"=="remove" goto :remove_migration
if /i "%COMMAND%"=="update" goto :update_database
if /i "%COMMAND%"=="apply" goto :update_database
if /i "%COMMAND%"=="list" goto :list_migrations

echo.
echo ERROR: Invalid command "%COMMAND%"
echo Valid commands: add, remove, update, apply, list
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
    echo Usage: ef.bat add ^<MigrationName^>
    pause
    exit /b 1
)
echo Adding migration: %MIGRATION_NAME%
dotnet ef migrations add %MIGRATION_NAME% --project %PERSISTENCE_PATH% --startup-project %MIGRATOR_PATH% --context AppDbContext
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
dotnet ef migrations remove --project %PERSISTENCE_PATH% --startup-project %MIGRATOR_PATH% --context AppDbContext --force 2>nul
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
dotnet ef migrations remove --project %PERSISTENCE_PATH% --startup-project %MIGRATOR_PATH% --context AppDbContext --force
goto :end

REM ============================================
REM  UPDATE DATABASE (via QLDA.Migrator)
REM  SQL Server → Database.Migrate() (migration files)
REM  SQLite → Database.EnsureCreated() (EF Core model)
REM ============================================
:update_database
echo Applying schema via QLDA.Migrator...
echo Provider: %PROVIDER%
dotnet run --project %MIGRATOR_PATH% -- --provider %PROVIDER%
goto :end

REM ============================================
REM  LIST MIGRATIONS
REM ============================================
:list_migrations
echo Listing migrations...
dotnet ef migrations list --project %PERSISTENCE_PATH% --startup-project %MIGRATOR_PATH% --context AppDbContext
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
