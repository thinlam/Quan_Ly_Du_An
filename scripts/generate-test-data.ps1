# generate-test-data.ps1 - Generate fake data using QLDA.DevSeeder
param(
    [int]$Count = 10,
    [string]$Type = "all",
    [string]$Output = "dev-data.db",
    [string]$ConnectionString = ""
)

$ErrorActionPreference = "Stop"

Write-Host "QLDA Test Data Generator" -ForegroundColor Cyan
Write-Host "  Count: $Count"
Write-Host "  Type:  $Type"
Write-Host "  Output: $Output"

$args = @("run", "--project", "QLDA.DevSeeder/QLDA.DevSeeder.csproj", "--", "seed", "-c", $Count, "-t", $Type, "-o", $Output)

if ($ConnectionString) {
    $args += @("--connection-string", $ConnectionString)
}

& dotnet @args

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nDone! Use the database file: $Output" -ForegroundColor Green
} else {
    Write-Host "`nFailed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}
