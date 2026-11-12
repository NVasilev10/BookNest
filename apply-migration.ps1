# Script to apply the migration to the BookNest database
# Run this from the solution root directory in PowerShell

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "BookNest - Applying Database Migration" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Check if dotnet EF tools are installed
$efCheck = dotnet ef --version
if ($LASTEXITCODE -ne 0) {
	Write-Host "Installing dotnet ef tools..." -ForegroundColor Yellow
	dotnet tool install --global dotnet-ef
}

# Navigate to the project directory
Write-Host "`nApplying migration: AddUserIdToReviews" -ForegroundColor Yellow

# Apply the migration
dotnet ef database update --project BookNest.csproj

if ($LASTEXITCODE -eq 0) {
	Write-Host "`n✅ Migration applied successfully!" -ForegroundColor Green
	Write-Host "The UserId column has been added to the Reviews table." -ForegroundColor Green
} else {
	Write-Host "`n❌ Migration failed. Please check the error messages above." -ForegroundColor Red
}

Write-Host "`nPress any key to close..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
