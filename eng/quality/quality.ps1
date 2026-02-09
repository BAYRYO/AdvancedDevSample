param(
    [double]$GlobalLineThreshold = 55,
    [double]$InfrastructureLineThreshold = 30
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '../..')
$solution = Join-Path $repoRoot 'AdvancedDevSample.slnx'
$resultsDir = Join-Path $repoRoot 'TestResults'
$runResultsDir = Join-Path $resultsDir ("quality-" + [DateTime]::UtcNow.ToString('yyyyMMddHHmmss') + "-" + [Guid]::NewGuid().ToString('N'))

Push-Location $repoRoot
try {
    dotnet restore $solution
    dotnet build $solution -nologo
    dotnet test $solution -nologo --collect:"XPlat Code Coverage" --settings (Join-Path $repoRoot 'eng/quality/coverage.runsettings') --results-directory $runResultsDir

    $coverageFile = Get-ChildItem -Path $runResultsDir -Recurse -Filter 'coverage.cobertura.xml' |
        Sort-Object LastWriteTimeUtc, FullName |
        Select-Object -Last 1 |
        Select-Object -ExpandProperty FullName

    if ([string]::IsNullOrWhiteSpace($coverageFile)) {
        throw ("Coverage file not found under {0}." -f $runResultsDir)
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
