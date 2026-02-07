param(
    [double]$GlobalLineThreshold = 55,
    [double]$InfrastructureLineThreshold = 30
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '../..')
$solution = Join-Path $repoRoot 'AdvancedDevSample.slnx'
$resultsDir = Join-Path $repoRoot 'TestResults'

Push-Location $repoRoot
try {
    dotnet restore $solution
    dotnet build $solution -nologo
    dotnet test $solution -nologo --collect:"XPlat Code Coverage" --results-directory $resultsDir

    $coverageFile = Get-ChildItem -Path $resultsDir -Recurse -Filter 'coverage.cobertura.xml' |
        Sort-Object FullName |
        Select-Object -Last 1 |
        Select-Object -ExpandProperty FullName

    if ([string]::IsNullOrWhiteSpace($coverageFile)) {
        throw 'Coverage file not found under TestResults.'
    }

    & (Join-Path $repoRoot 'eng/quality/check-coverage.ps1') `
        -CoverageFile $coverageFile `
        -GlobalLineThreshold $GlobalLineThreshold `
        -InfrastructureLineThreshold $InfrastructureLineThreshold

    dotnet format $solution --verify-no-changes --severity error --verbosity minimal
}
finally {
    Pop-Location
}
