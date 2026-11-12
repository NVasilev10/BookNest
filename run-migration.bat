@echo off
REM Complete automation script for BookNest migration
REM All steps in one go

echo.
echo ================================================
echo BookNest - Complete Migration Setup
echo ================================================
echo.

REM Step 1: Build the project
echo Step 1: Building the project...
dotnet build
if errorlevel 1 (
	echo Build failed! Exiting...
	pause
	exit /b 1
)

echo.
echo Step 2: Creating migration...
echo.

REM Step 2: Add migration
dotnet ef migrations add AddUserIdToModels
if errorlevel 1 (
	echo Migration creation failed! Exiting...
	pause
	exit /b 1
)

echo.
echo Step 3: Applying migration to database...
echo.

REM Step 3: Update database
dotnet ef database update
if errorlevel 1 (
	echo Database update failed! Exiting...
	pause
	exit /b 1
)

echo.
echo ================================================
echo SUCCESS! All steps completed:
echo.
echo 1. ✅ Project built
echo 2. ✅ Migration created (AddUserIdToModels)
echo 3. ✅ Database updated
echo.
echo The UserId column has been added to Reviews and BookCollections tables.
echo Your application should now work without errors!
echo ================================================
echo.
pause
