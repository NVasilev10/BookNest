@echo off
REM Script to apply the migration to the BookNest database
REM Run this from the solution root directory

echo.
echo ================================================
echo BookNest - Applying Database Migration
echo ================================================
echo.

REM Check if dotnet EF tools are installed
echo Checking for dotnet ef tools...
dotnet ef --version >nul 2>&1
if errorlevel 1 (
	echo Installing dotnet ef tools...
	dotnet tool install --global dotnet-ef
)

REM Apply the migration
echo.
echo Applying migration: AddUserIdToReviews
echo.
dotnet ef database update --project BookNest.csproj

if errorlevel 1 (
	echo.
	echo ================================================
	echo ERROR: Migration failed. Please check the error messages above.
	echo ================================================
) else (
	echo.
	echo ================================================
	echo SUCCESS: Migration applied successfully!
	echo The UserId column has been added to the Reviews table.
	echo ================================================
)

echo.
pause
