# Google Authentication Setup Script for Unistay
# This script helps you set up Google OAuth authentication

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Unistay - Google Auth Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet is installed
Write-Host "Checking .NET installation..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ .NET SDK $dotnetVersion is installed" -ForegroundColor Green
} else {
    Write-Host "✗ .NET SDK is not installed. Please install .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 1: Restore NuGet packages" -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Configure Google OAuth Credentials" -ForegroundColor Yellow
Write-Host "You need to get your Google OAuth credentials from:" -ForegroundColor White
Write-Host "https://console.cloud.google.com/" -ForegroundColor Cyan
Write-Host ""

$useUserSecrets = Read-Host "Do you want to use User Secrets to store credentials? (Y/N)"

if ($useUserSecrets -eq "Y" -or $useUserSecrets -eq "y") {
    Write-Host ""
    Write-Host "Initializing User Secrets..." -ForegroundColor Yellow
    dotnet user-secrets init
    
    Write-Host ""
    $clientId = Read-Host "Enter your Google Client ID"
    $clientSecret = Read-Host "Enter your Google Client Secret" -AsSecureString
    $clientSecretPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($clientSecret))
    
    dotnet user-secrets set "Authentication:Google:ClientId" $clientId
    dotnet user-secrets set "Authentication:Google:ClientSecret" $clientSecretPlain
    
    Write-Host "✓ Credentials saved to User Secrets" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Please manually update appsettings.json with your credentials:" -ForegroundColor Yellow
    Write-Host '  "Authentication": {' -ForegroundColor White
    Write-Host '    "Google": {' -ForegroundColor White
    Write-Host '      "ClientId": "YOUR_CLIENT_ID_HERE",' -ForegroundColor White
    Write-Host '      "ClientSecret": "YOUR_CLIENT_SECRET_HERE"' -ForegroundColor White
    Write-Host '    }' -ForegroundColor White
    Write-Host '  }' -ForegroundColor White
}

Write-Host ""
Write-Host "Step 3: Database Migration" -ForegroundColor Yellow
$createMigration = Read-Host "Do you want to create a new migration for Identity? (Y/N)"

if ($createMigration -eq "Y" -or $createMigration -eq "y") {
    Write-Host "Creating migration..." -ForegroundColor Yellow
    dotnet ef migrations add AddGoogleAuthentication
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Migration created successfully" -ForegroundColor Green
        
        $updateDb = Read-Host "Do you want to update the database now? (Y/N)"
        if ($updateDb -eq "Y" -or $updateDb -eq "y") {
            Write-Host "Updating database..." -ForegroundColor Yellow
            dotnet ef database update
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Database updated successfully" -ForegroundColor Green
            } else {
                Write-Host "✗ Failed to update database" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "✗ Failed to create migration" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Make sure you've configured Google OAuth in Google Cloud Console" -ForegroundColor White
Write-Host "2. Add authorized redirect URIs:" -ForegroundColor White
Write-Host "   - https://localhost:7000/signin-google" -ForegroundColor Cyan
Write-Host "   - http://localhost:5000/signin-google" -ForegroundColor Cyan
Write-Host "3. Run the application: dotnet run" -ForegroundColor White
Write-Host "4. Navigate to: https://localhost:7000/Account/Login" -ForegroundColor White
Write-Host "5. Click 'Đăng nhập với Google' to test" -ForegroundColor White
Write-Host ""
Write-Host "For detailed instructions, see: GOOGLE_LOGIN_SETUP.md" -ForegroundColor Yellow
Write-Host ""

$runApp = Read-Host "Do you want to run the application now? (Y/N)"
if ($runApp -eq "Y" -or $runApp -eq "y") {
    Write-Host ""
    Write-Host "Starting application..." -ForegroundColor Yellow
    dotnet run
}
