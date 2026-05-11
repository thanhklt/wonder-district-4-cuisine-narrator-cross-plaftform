param(
    [string]$BaseUrl = "http://localhost:5184",
    [string]$Path = "/api/Poi",
    [string]$OutputFile = "load_test_log.txt"
)

$ErrorActionPreference = "Stop"

$scriptPath = Join-Path $PSScriptRoot "load-test.ps1"

if (-not (Test-Path $scriptPath)) {
    throw "Cannot find load-test.ps1 at $scriptPath"
}

$scenarios = @(
    @{ Name = "Warmup"; Requests = 20;  Concurrency = 2  },
    @{ Name = "Light";  Requests = 50;  Concurrency = 5  },
    @{ Name = "Normal"; Requests = 100; Concurrency = 10 },
    @{ Name = "High";   Requests = 150; Concurrency = 20 },
    @{ Name = "Stress"; Requests = 200; Concurrency = 50 }
)

$startedAt = Get-Date

@"
LOAD TEST LOG
=============

Base URL   : $BaseUrl
Path       : $Path
Started at : $($startedAt.ToString("yyyy-MM-dd HH:mm:ss"))

"@ | Set-Content -Path $OutputFile -Encoding UTF8

foreach ($scenario in $scenarios) {
    $scenarioName = $scenario.Name
    $requests = $scenario.Requests
    $concurrency = $scenario.Concurrency
    $tempFile = Join-Path $PSScriptRoot "load_test_$($scenarioName.ToLower()).tmp.txt"

    Write-Host ""
    Write-Host "Running scenario: $scenarioName ($requests requests, concurrency $concurrency)"

    @"

================================================================================
SCENARIO: $scenarioName
Requests: $requests
Concurrency: $concurrency
================================================================================

"@ | Add-Content -Path $OutputFile -Encoding UTF8

    & $scriptPath `
        -BaseUrl $BaseUrl `
        -Path $Path `
        -TotalRequests $requests `
        -Concurrency $concurrency `
        -OutputFile $tempFile

    Get-Content -Path $tempFile | Add-Content -Path $OutputFile -Encoding UTF8

    Remove-Item -Path $tempFile -Force

    Start-Sleep -Seconds 3
}

$finishedAt = Get-Date

@"

================================================================================
DONE
================================================================================

Finished at : $($finishedAt.ToString("yyyy-MM-dd HH:mm:ss"))
Total time  : $([Math]::Round(($finishedAt - $startedAt).TotalSeconds, 2))s
"@ | Add-Content -Path $OutputFile -Encoding UTF8

Write-Host ""
Write-Host "All load tests completed. Log saved to $OutputFile"
