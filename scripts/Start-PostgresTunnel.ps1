#Requires -Version 7
<#
.SYNOPSIS
  Avvia il tunnel SSH che inoltra localhost:5432 -> Podman WSL postgres:5432.
  Workaround per bug WSL2 localhostForwarding con Podman rootful (NAT).
  Richiede Podman machine running e container post-postgres.
#>
param(
  [int]$ListenPort = 5432,
  [int]$SshPort = 49690
)

$ErrorActionPreference = 'Stop'

$identity = "$env:USERPROFILE\.local\share\containers\podman\machine\machine"
if (-not (Test-Path -LiteralPath $identity)) {
  throw "Chiave Podman non trovata: $identity. Esegui 'podman machine init'."
}

# Chiude tunnel precedenti sulla stessa porta (opzionale)
$existing = Get-NetTCPConnection -LocalPort $ListenPort -ErrorAction SilentlyContinue | Where-Object { $_.State -eq 'Listen' }
if ($existing) {
  Write-Host "Trovato listener esistente su $ListenPort (PID $($existing.OwningProcess)). Tentativo di riuso..." -ForegroundColor Yellow
  $tcp = Test-NetConnection -ComputerName 127.0.0.1 -Port $ListenPort -WarningAction SilentlyContinue
  if ($tcp.TcpTestSucceeded) {
    Write-Host "Tunnel già attivo — nessuna azione." -ForegroundColor Green
    exit 0
  }
  Write-Host "Listener non risponde, proseguo..." -ForegroundColor Yellow
}

# Verifica Podman
$machineList = podman machine list 2>&1 | Out-String
if ($machineList -notmatch 'Currently running') {
  Write-Host "Avvio podman-machine-default..." -ForegroundColor Cyan
  podman machine start | Write-Host
  Start-Sleep -Seconds 5
}

$ps = podman ps --filter "name=post-postgres" --format "{{.Status}}" 2>&1 | Out-String
if ($ps -notmatch 'Up') {
  Write-Host "Avvio container post-postgres..." -ForegroundColor Cyan
  podman start post-postgres | Write-Host
  Start-Sleep -Seconds 4
}

$wslIp = (podman machine ssh -- "ip -4 addr show eth0 | grep -oP '(?<=inet\s)\d+(\.\d+){3}' | head -1" 2>&1 | Select-Object -First 1).ToString().Trim()
Write-Host "WSL IP: $wslIp" -ForegroundColor DarkGray

# Avvia tunnel via OpenSSH (senza richiedere admin come netsh portproxy)
$sshArgs = "-i `"$identity`" -o StrictHostKeyChecking=no -o UserKnownHostsFile=NUL -o ExitOnForwardFailure=yes -o ServerAliveInterval=30 -p $SshPort -L ${ListenPort}:localhost:${ListenPort} user@localhost -N"
Write-Host "Avvio tunnel: ssh $sshArgs" -ForegroundColor Cyan
Start-Process -FilePath "C:\Windows\System32\OpenSSH\ssh.exe" -ArgumentList $sshArgs -WindowStyle Hidden

Start-Sleep -Seconds 3
$test = Test-NetConnection -ComputerName 127.0.0.1 -Port $ListenPort -WarningAction SilentlyContinue
if ($test.TcpTestSucceeded) {
  Write-Host "Tunnel attivo: 127.0.0.1:$ListenPort -> ${wslIp}:$ListenPort" -ForegroundColor Green
  Write-Host "Verifica: Test-NetConnection localhost -Port $ListenPort dovrebbe essere True" -ForegroundColor Green
} else {
  Write-Warning "Tunnel non raggiungibile. Verifica con: netstat -ano | Select-String $ListenPort"
  exit 1
}

# Alternativa senza tunnel (richiede di cambiare connection string):
Write-Host "`nAlternativa senza tunnel (se non vuoi tenere il processo ssh):" -ForegroundColor DarkGray
Write-Host "  Aggiorna src/PostForge.Api/appsettings.Development.json:10 con Host=$wslIp" -ForegroundColor DarkGray
Write-Host "  Nota: l'IP WSL cambia ad ogni reboot (wsl --shutdown)." -ForegroundColor DarkGray
