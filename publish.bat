@echo off
REM RepoKit NuGet Publish Script for Windows
REM This script builds and publishes RepoKit packages to NuGet.org

setlocal enabledelayedexpansion

echo.
echo 🚀 RepoKit NuGet Publish Script (Windows)
echo =========================================
echo.

REM Check if API key is provided
if "%1"=="" (
    echo ❌ Error: NuGet API key not provided
    echo.
    echo Usage: publish.bat ^<NuGet_API_Key^>
    echo.
    echo Example:
    echo   publish.bat oy2j3k4jh5k6j7k8...
    echo.
    exit /b 1
)

set "API_KEY=%1"

echo 📋 Pre-flight checks...
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Error: dotnet CLI not found
    exit /b 1
)

echo ✅ dotnet CLI found: 
dotnet --version
echo.

REM Clean previous builds
echo 🧹 Cleaning previous builds...
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "src\RepoKit.Core\bin\Release" rmdir /s /q "src\RepoKit.Core\bin\Release"
if exist "src\RepoKit.Dapper\bin\Release" rmdir /s /q "src\RepoKit.Dapper\bin\Release"
if exist "src\RepoKit.EfCore\bin\Release" rmdir /s /q "src\RepoKit.EfCore\bin\Release"
echo ✅ Cleaned
echo.

REM Build solution
echo 🔨 Building solution in Release mode...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo ❌ Build failed
    exit /b 1
)
echo ✅ Build successful
echo.

REM Run tests
echo 🧪 Running tests...
dotnet test -c Release --no-build
if %errorlevel% neq 0 (
    echo ❌ Tests failed
    exit /b 1
)
echo ✅ All tests passed
echo.

REM Create packages
echo 📦 Creating NuGet packages...
dotnet pack -c Release -o ./bin/Release
if %errorlevel% neq 0 (
    echo ❌ Pack failed
    exit /b 1
)
echo ✅ Packages created:
dir "bin\Release\*.nupkg"
echo.

REM List all packages
echo 📦 Packages to be published:
for %%F in (bin\Release\*.nupkg) do (
    if not "%%F"=="*.snupkg" (
        echo   - %%~nxF
    )
)
echo.

REM Confirmation
set /p "CONFIRM=🤔 Proceed with publishing? (yes/no): "
if /i not "%CONFIRM%"=="yes" (
    echo ❌ Publish cancelled
    exit /b 1
)

echo.
echo 🌐 Publishing to NuGet.org...
echo.

REM Publish each package
for %%F in (bin\Release\*.nupkg) do (
    if not "%%F"=="*.snupkg" (
        echo 📤 Publishing %%~nxF...
        dotnet nuget push "%%F" ^
            --api-key %API_KEY% ^
            --source https://api.nuget.org/v3/index.json ^
            --skip-duplicate
        
        if %errorlevel% equ 0 (
            echo ✅ %%~nxF published successfully
        ) else (
            echo ❌ Failed to publish %%~nxF
            exit /b 1
        )
    )
)

echo.
echo ✨ Publishing complete!
echo.
echo 📊 Verification:
echo   RepoKit.Core: https://www.nuget.org/packages/RepoKit.Core
echo   RepoKit.EfCore: https://www.nuget.org/packages/RepoKit.EfCore
echo   RepoKit.Dapper: https://www.nuget.org/packages/RepoKit.Dapper
echo.
echo 🎉 Done!
