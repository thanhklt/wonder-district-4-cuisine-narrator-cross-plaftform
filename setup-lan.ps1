param(
    [Parameter(Mandatory=$true)]
    [string]$IP
)

$ErrorActionPreference = "Stop"
Write-Host "Setting up LAN for IP: $IP" -ForegroundColor Cyan

# 1. Cap nhat HOST_IP trong .env
$envPath = Join-Path $PSScriptRoot ".env"
if (-not (Test-Path $envPath)) {
    Write-Host "ERROR: .env file not found at $envPath" -ForegroundColor Red
    exit 1
}
(Get-Content $envPath) -replace "HOST_IP=.*", "HOST_IP=$IP" | Set-Content $envPath
Write-Host "[1/4] Updated HOST_IP in .env" -ForegroundColor Green

# 2. Tao lai SSL cert cho IP moi
$sslDir = Join-Path $PSScriptRoot "nginx\ssl"
if (-not (Test-Path $sslDir)) {
    New-Item -ItemType Directory -Path $sslDir | Out-Null
}
Push-Location $sslDir
try {
    Remove-Item cert.pem, key.pem -ErrorAction SilentlyContinue
    mkcert $IP localhost 127.0.0.1
    # mkcert tao file co ten dang: <IP>+2.pem va <IP>+2-key.pem
    $certFile = Get-ChildItem -Filter "$IP*.pem" | Where-Object { $_.Name -notmatch "key" } | Select-Object -First 1
    $keyFile  = Get-ChildItem -Filter "$IP*-key.pem" | Select-Object -First 1
    if ($certFile) { Rename-Item $certFile.FullName "cert.pem" -Force }
    if ($keyFile)  { Rename-Item $keyFile.FullName  "key.pem"  -Force }
    Write-Host "[2/4] SSL cert generated for $IP" -ForegroundColor Green
} finally {
    Pop-Location
}

# 3. Rebuild va restart Docker
Push-Location $PSScriptRoot
try {
    docker compose down
    Write-Host "[3/4] Docker stopped" -ForegroundColor Green
    docker compose up --build -d
    Write-Host "[4/4] Docker started" -ForegroundColor Green
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "Done! Truy cap tu dien thoai: https://$IP" -ForegroundColor Yellow
Write-Host "Web Admin: https://${IP}:3001" -ForegroundColor Yellow
