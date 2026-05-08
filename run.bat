@echo off
set USE_SQLITE=0
if "%~1"=="--sqlite" set USE_SQLITE=1
if "%~2"=="--sqlite" set USE_SQLITE=1

if "%USE_SQLITE%"=="1" (
    cd QLDA.WebApi && dotnet run -- --provider sqlite && cd ..
) else (
    cd QLDA.WebApi && dotnet run && cd ..
)
