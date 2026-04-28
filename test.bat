@echo off
:: Test runner for QLDA
:: Usage: test.bat [filter]
:: Examples:
::   test.bat                    - Run all tests
::   test.bat build              - Build only (no tests)
::   test.bat int                - Run integration tests only
::   test.bat duan               - Run DuAn tests
::   test.bat goithau            - Run GoiThau tests
::   test.bat hopdong            - Run HopDong tests
::   test.bat detailed           - Run all tests with detailed output

set PROJECT=QLDA.Tests\QLDA.Tests.csproj

if "%~1"=="" goto all
if "%~1"=="build" goto build
if "%~1"=="int" goto integration
if "%~1"=="duan" goto duan
if "%~1"=="goithau" goto goithau
if "%~1"=="hopdong" goto hopdong
if "%~1"=="detailed" goto detailed
goto filter

:all
dotnet test %PROJECT% --logger "console;verbosity=normal"
goto end

:build
dotnet build SER.sln
goto end

:integration
dotnet test %PROJECT% --filter "FullyQualifiedName~Integration" --logger "console;verbosity=detailed"
goto end

:duan
dotnet test %PROJECT% --filter "DuAnControllerTests" --logger "console;verbosity=detailed"
goto end

:goithau
dotnet test %PROJECT% --filter "GoiThauControllerTests" --logger "console;verbosity=detailed"
goto end

:hopdong
dotnet test %PROJECT% --filter "HopDongControllerTests" --logger "console;verbosity=detailed"
goto end

:detailed
dotnet test %PROJECT% --logger "console;verbosity=detailed"
goto end

:filter
dotnet test %PROJECT% --filter "%~1" --logger "console;verbosity=detailed"
goto end

:end
