# PowerShell script to run tests with coverage and generate reports

Write-Host "Running digioz.Portal.Tests with coverage..." -ForegroundColor Green

# Clean previous coverage results
if (Test-Path ".\TestResults") {
    Remove-Item ".\TestResults" -Recurse -Force
    Write-Host "Cleaned previous test results" -ForegroundColor Yellow
}

# Run tests with coverage collection
Write-Host "`nRunning tests..." -ForegroundColor Green
dotnet test `
    --configuration Release `
    --logger "console;verbosity=detailed" `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=opencover `
    /p:CoverletOutput=./TestResults/coverage.opencover.xml `
    /p:ExcludeByFile="**/*Designer.cs,**/*.g.cs,**/*.g.i.cs" `
    /p:Exclude="[*]*.Migrations.*"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nTests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nTests completed successfully!" -ForegroundColor Green

# Check if ReportGenerator is installed
$reportGenerator = Get-Command reportgenerator -ErrorAction SilentlyContinue

if ($null -eq $reportGenerator) {
    Write-Host "`nReportGenerator not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

# Generate HTML coverage report
if (Test-Path ".\TestResults\coverage.opencover.xml") {
    Write-Host "`nGenerating coverage report..." -ForegroundColor Green
    
    reportgenerator `
        -reports:".\TestResults\coverage.opencover.xml" `
        -targetdir:".\TestResults\CoverageReport" `
        -reporttypes:"Html;Badges;TextSummary" `
        -title:"digioz.Portal Test Coverage"
    
    Write-Host "`nCoverage report generated at: .\TestResults\CoverageReport\index.html" -ForegroundColor Green
    
    # Display summary
    if (Test-Path ".\TestResults\CoverageReport\Summary.txt") {
        Write-Host "`n=== Coverage Summary ===" -ForegroundColor Cyan
        Get-Content ".\TestResults\CoverageReport\Summary.txt"
    }
    
    # Open report in browser
    $openReport = Read-Host "`nOpen coverage report in browser? (Y/N)"
    if ($openReport -eq "Y" -or $openReport -eq "y") {
        Start-Process ".\TestResults\CoverageReport\index.html"
    }
} else {
    Write-Host "`nCoverage file not found!" -ForegroundColor Red
}

Write-Host "`nDone!" -ForegroundColor Green
