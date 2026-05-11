param(
    [string]$BaseUrl = "http://localhost:5184",
    [string]$Path = "/api/Poi",
    [int]$TotalRequests = 150,
    [int]$Concurrency = 20,
    [string]$OutputFile = "load_test.txt"
)

$ErrorActionPreference = "Stop"

$targetUrl = $BaseUrl.TrimEnd("/") + "/" + $Path.TrimStart("/")
$startedAt = Get-Date
$timer = [System.Diagnostics.Stopwatch]::StartNew()

Write-Host "Load testing $targetUrl"
Write-Host "Total requests: $TotalRequests"
Write-Host "Concurrency: $Concurrency"
Write-Host "Output: $OutputFile"

$pool = [runspacefactory]::CreateRunspacePool(1, $Concurrency)
$pool.Open()

$jobs = New-Object System.Collections.Generic.List[object]

for ($i = 1; $i -le $TotalRequests; $i++) {
    $ps = [powershell]::Create()
    $ps.RunspacePool = $pool

    [void]$ps.AddScript({
        param($Url, $RequestNumber)

        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        try {
            $response = Invoke-WebRequest -Uri $Url -Method GET -UseBasicParsing -TimeoutSec 30
            $sw.Stop()

            [pscustomobject]@{
                RequestNumber = $RequestNumber
                StatusCode = [int]$response.StatusCode
                Success = $true
                DurationMs = $sw.ElapsedMilliseconds
                Error = ""
            }
        }
        catch {
            $sw.Stop()
            $statusCode = 0

            if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
                $statusCode = [int]$_.Exception.Response.StatusCode
            }

            [pscustomobject]@{
                RequestNumber = $RequestNumber
                StatusCode = $statusCode
                Success = $false
                DurationMs = $sw.ElapsedMilliseconds
                Error = $_.Exception.Message
            }
        }
    })

    [void]$ps.AddArgument($targetUrl)
    [void]$ps.AddArgument($i)

    $jobs.Add([pscustomobject]@{
        PowerShell = $ps
        Handle = $ps.BeginInvoke()
    })
}

$results = New-Object System.Collections.Generic.List[object]

foreach ($job in $jobs) {
    $output = $job.PowerShell.EndInvoke($job.Handle)
    foreach ($item in $output) {
        $results.Add($item)
    }
    $job.PowerShell.Dispose()
}

$timer.Stop()
$pool.Close()
$pool.Dispose()

$finishedAt = Get-Date
$successCount = ($results | Where-Object { $_.Success }).Count
$failedCount = $results.Count - $successCount
$durations = @($results | ForEach-Object { $_.DurationMs } | Sort-Object)

function Get-Percentile {
    param(
        [long[]]$Values,
        [double]$Percentile
    )

    if ($Values.Count -eq 0) {
        return 0
    }

    $index = [Math]::Ceiling(($Percentile / 100) * $Values.Count) - 1
    $index = [Math]::Max(0, [Math]::Min($index, $Values.Count - 1))
    return $Values[$index]
}

$averageMs = 0
if ($durations.Count -gt 0) {
    $averageMs = [Math]::Round(($durations | Measure-Object -Average).Average, 2)
}

$requestsPerSecond = 0
if ($timer.Elapsed.TotalSeconds -gt 0) {
    $requestsPerSecond = [Math]::Round($TotalRequests / $timer.Elapsed.TotalSeconds, 2)
}

$statusGroups = $results |
    Group-Object StatusCode |
    Sort-Object Name |
    ForEach-Object { "HTTP $($_.Name): $($_.Count)" }

$failedSamples = $results |
    Where-Object { -not $_.Success } |
    Select-Object -First 10 |
    ForEach-Object { "#$($_.RequestNumber) HTTP $($_.StatusCode) $($_.DurationMs)ms - $($_.Error)" }

$report = @"
LOAD TEST REPORT
================

Target URL       : $targetUrl
Started at       : $($startedAt.ToString("yyyy-MM-dd HH:mm:ss"))
Finished at      : $($finishedAt.ToString("yyyy-MM-dd HH:mm:ss"))
Total time       : $([Math]::Round($timer.Elapsed.TotalSeconds, 2))s

Total requests   : $TotalRequests
Concurrency      : $Concurrency
Successful       : $successCount
Failed           : $failedCount
Requests/sec     : $requestsPerSecond

Latency
-------
Average          : $averageMs ms
Min              : $(Get-Percentile $durations 0) ms
P50              : $(Get-Percentile $durations 50) ms
P90              : $(Get-Percentile $durations 90) ms
P95              : $(Get-Percentile $durations 95) ms
P99              : $(Get-Percentile $durations 99) ms
Max              : $(Get-Percentile $durations 100) ms

Status Codes
------------
$($statusGroups -join "`r`n")

Failed Samples
--------------
$($failedSamples -join "`r`n")
"@

$report | Set-Content -Path $OutputFile -Encoding UTF8

Write-Host ""
Write-Host $report
