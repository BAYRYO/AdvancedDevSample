param(
    [string]$CoverageFile,
    [double]$GlobalLineThreshold = 55,
    [double]$InfrastructureLineThreshold = 30
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($CoverageFile)) {
    $CoverageFile = Get-ChildItem -Path '.' -Recurse -Filter 'coverage.cobertura.xml' |
        Sort-Object LastWriteTimeUtc, FullName |
        Select-Object -Last 1 |
        Select-Object -ExpandProperty FullName
}

if ([string]::IsNullOrWhiteSpace($CoverageFile) -or -not (Test-Path -LiteralPath $CoverageFile)) {
    Write-Error 'Coverage file not found. Run tests with --collect:"XPlat Code Coverage" first.'
}

[xml]$coverageXml = Get-Content -LiteralPath $CoverageFile -Raw

$globalLineRate = [double]$coverageXml.coverage.'line-rate' * 100
$infrastructurePackage = $coverageXml.coverage.packages.package |
    Where-Object { $_.name -eq 'AdvancedDevSample.Infrastructure' } |
    Select-Object -First 1

if ($null -eq $infrastructurePackage) {
    Write-Error 'Package AdvancedDevSample.Infrastructure was not found in coverage report.'
}

$infrastructureLineRate = [double]$infrastructurePackage.'line-rate' * 100

Write-Host ("Coverage file: {0}" -f $CoverageFile)
Write-Host ''
Write-Host 'Coverage thresholds:'
Write-Host ("  Global line rate:         {0:N2}% (required >= {1:N2}%)" -f $globalLineRate, $GlobalLineThreshold)
Write-Host ("  Infrastructure line rate: {0:N2}% (required >= {1:N2}%)" -f $infrastructureLineRate, $InfrastructureLineThreshold)
Write-Host ''

$failed = $false

if ($globalLineRate -lt $GlobalLineThreshold) {
    Write-Host ("FAIL: global line rate is below threshold.") -ForegroundColor Red
    $failed = $true
}

if ($infrastructureLineRate -lt $InfrastructureLineThreshold) {
    Write-Host ("FAIL: infrastructure line rate is below threshold.") -ForegroundColor Red
    $failed = $true
}

if ($failed) {
    exit 1
}

Write-Host 'PASS: coverage thresholds satisfied.' -ForegroundColor Green
