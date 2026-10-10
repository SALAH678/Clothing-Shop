<#
.SYNOPSIS
    One-shot setup for the Clothing Shop stack.

.DESCRIPTION
    Automates steps 0-5 of "Running the Application" in README.md:

      0. Creates .env and frontend/.env from the committed .example files
         (existing files are never overwritten).
      1. Generates/trusts the ASP.NET HTTPS development certificate and
         exports it to %USERPROFILE%\.aspnet\https\aspnetapp.pfx.
      2. Generates the frontend HTTPS certificates with mkcert
         (frontend/localhost+2.pem and frontend/localhost+2-key.pem).
      3. Fills PASSWORD, NGROK_AUTHTOKEN, NGROK_DOMAIN and SECRET_KEY into
         .env. Only the values you can supply are prompted for; SECRET_KEY
         is generated automatically.
      4. Starts the stack with "docker compose up --build -d".
      5. Prints the URLs of every service and the demo accounts.

    The script is idempotent: at every prompt, pressing Enter keeps the
    value already stored in .env, and files that already exist are reused
    (the existing .pfx is re-generated only when its password no longer
    matches what you supply).

.PARAMETER CertificatePassword
    Password for the ASP.NET HTTPS certificate (written to PASSWORD in
    .env). Prompted interactively when omitted.

.PARAMETER NgrokAuthtoken
    ngrok authtoken (written to NGROK_AUTHTOKEN in .env). Prompted
    interactively when omitted.

.PARAMETER NgrokDomain
    Reserved ngrok domain, hostname only, e.g. my-store.ngrok-free.app
    (written to NGROK_DOMAIN in .env). Prompted interactively when omitted.

.PARAMETER SkipTrust
    Skip "dotnet dev-certs https --trust". The script already skips the
    trust step automatically on Linux, where .NET does not support it.

.PARAMETER SkipStart
    Do everything except the final "docker compose up --build -d".

.EXAMPLE
    .\scripts\setup.ps1
    Interactive setup; press Enter at every prompt to accept the values
    already present in .env.

.EXAMPLE
    .\scripts\setup.ps1 -SkipStart
    Prepare everything (env files, certificates, .env) without starting
    the stack.
#>

[CmdletBinding()]
param(
    [string]$CertificatePassword,
    [string]$NgrokAuthtoken,
    [string]$NgrokDomain,
    [switch]$SkipTrust,
    [switch]$SkipStart
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$EnvFile = Join-Path $RepoRoot '.env'
$EnvExampleFile = Join-Path $RepoRoot '.env.example'
$FrontendDir = Join-Path $RepoRoot 'frontend'
$FrontendEnvFile = Join-Path $FrontendDir '.env'
$FrontendEnvExampleFile = Join-Path $FrontendDir '.env.example'

# -----------------------------------------------------------------------------
# Helpers
# -----------------------------------------------------------------------------

function Write-Step {
    param([string]$Title)
    Write-Host ''
    Write-Host ('=' * 70)
    Write-Host "==> $Title"
    Write-Host ('=' * 70)
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)][string]$File,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [string]$WorkingDirectory,
        [switch]$WarnOnError
    )
    $locationPushed = $false
    if ($WorkingDirectory) {
        Push-Location $WorkingDirectory
        $locationPushed = $true
    }
    try {
        Write-Verbose "$File $($Arguments -join ' ')"
        # Run native commands with EAP=Continue so their stderr output cannot
        # surface as a terminating NativeCommandError in Windows PowerShell 5.1;
        # the exit code below is the real pass/fail signal.
        $previousEap = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        try {
            # 2>&1 | Write-Host: Windows PowerShell 5.1 would otherwise turn
            # stderr lines into decorated error records when EAP=Continue.
            & $File @Arguments 2>&1 | ForEach-Object { Write-Host $_ }
            $exitCode = $LASTEXITCODE
        }
        finally {
            $ErrorActionPreference = $previousEap
        }
        if ($exitCode -ne 0) {
            $message = "'$File $($Arguments -join ' ')' failed with exit code $exitCode."
            if ($WarnOnError) { Write-Warning $message }
            else { throw $message }
        }
    }
    finally {
        if ($locationPushed) { Pop-Location }
    }
}

function Assert-Command {
    param([string]$Name, [string]$Hint)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "'$Name' was not found on PATH. $Hint"
    }
}

function Get-EnvValue {
    param([string]$Path, [string]$Key)
    $line = Get-Content $Path | Where-Object { $_ -match "^\s*$([regex]::Escape($Key))=" } | Select-Object -First 1
    if ($null -eq $line) { return $null }
    return ($line -split '=', 2)[1]
}

function Set-EnvValue {
    param([string]$Path, [string]$Key, [string]$Value)
    $lines = Get-Content $Path
    $found = $false
    $out = foreach ($line in $lines) {
        if ($line -match "^\s*$([regex]::Escape($Key))=") {
            $found = $true
            "$Key=$Value"
        }
        else {
            $line
        }
    }
    if (-not $found) { $out += "$Key=$Value" }
    Set-Content -Path $Path -Value $out
}

function Test-Placeholder {
    param([string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return $true }
    return $Value -match '^(choose-|replace-|your-)'
}

function New-SecretKey {
    $bytes = New-Object byte[] 64
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) }
    finally { $rng.Dispose() }
    return [Convert]::ToBase64String($bytes)
}

function Test-PfxPassword {
    param([string]$Path, [string]$Password)
    try {
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($Path, $Password)
        $cert.Dispose()
        return $true
    }
    catch {
        return $false
    }
}

# -----------------------------------------------------------------------------
# 0. Prerequisites
# -----------------------------------------------------------------------------

Write-Step 'Checking prerequisites'

Assert-Command -Name 'dotnet' -Hint 'Install the .NET 10 SDK: https://dotnet.microsoft.com/download'
Assert-Command -Name 'mkcert' -Hint 'Install mkcert: https://github.com/FiloSottile/mkcert'
Assert-Command -Name 'docker' -Hint 'Install Docker Desktop: https://www.docker.com/products/docker-desktop/'

# Probe under EAP=Continue with stderr merged away (2>&1 | Out-Null):
# Windows PowerShell 5.1 wraps a stopped daemon's stderr in ErrorRecords that
# terminate the script under EAP=Stop even when redirected, and print
# decorated noise when not. The exit codes below are the real signals.
$previousEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
docker compose version 2>&1 | Out-Null
$composeVersionOk = $LASTEXITCODE -eq 0
docker info 2>&1 | Out-Null
$dockerDaemonOk = $LASTEXITCODE -eq 0
$ErrorActionPreference = $previousEap

if (-not $composeVersionOk) {
    throw "'docker compose' (Compose v2) is not available. Update Docker Desktop or install the Compose v2 plugin."
}
if (-not $dockerDaemonOk) {
    if ($SkipStart) {
        Write-Warning 'Docker is not running - continuing because -SkipStart was supplied, but "docker compose up" will need the daemon later.'
    }
    else {
        throw 'Docker is not running. Start Docker Desktop and run this script again.'
    }
}

Write-Host 'All prerequisites found (dotnet, mkcert, docker + compose v2).'

# Compose mounts ${USERPROFILE}/.aspnet/https on every OS. On Windows
# USERPROFILE is set by the system; under pwsh on macOS/Linux export it for
# the duration of this run so "docker compose up" below works unchanged.
if (-not $env:USERPROFILE) {
    $env:USERPROFILE = $HOME
    Write-Host "Exported USERPROFILE=$HOME for this session (add 'export USERPROFILE=`$HOME' to your shell profile to run docker compose yourself later)."
}

# -----------------------------------------------------------------------------
# 1. Environment files (README step 0)
# -----------------------------------------------------------------------------

Write-Step 'Step 0 - Creating the environment files'

if (Test-Path $EnvFile) {
    Write-Host '.env already exists - keeping it.'
}
else {
    Copy-Item $EnvExampleFile $EnvFile
    Write-Host '.env created from .env.example.'
}

if (Test-Path $FrontendEnvFile) {
    Write-Host 'frontend/.env already exists - keeping it.'
}
else {
    Copy-Item $FrontendEnvExampleFile $FrontendEnvFile
    Write-Host 'frontend/.env created from frontend/.env.example.'
}

# -----------------------------------------------------------------------------
# 2. Collect the values only you can supply (README step 3)
# -----------------------------------------------------------------------------

Write-Step 'Step 3 - Collecting configuration values'

if ([string]::IsNullOrEmpty($CertificatePassword)) {
    $currentPassword = Get-EnvValue -Path $EnvFile -Key 'PASSWORD'
    $CertificatePassword = Read-Host 'Certificate password (becomes PASSWORD in .env) [Enter = keep current value]'
    if ([string]::IsNullOrEmpty($CertificatePassword)) { $CertificatePassword = $currentPassword }
}
if (Test-Placeholder $CertificatePassword) {
    throw 'A certificate password is required - it must match the password used to generate the .pfx below.'
}

if ([string]::IsNullOrEmpty($NgrokAuthtoken)) {
    $currentToken = Get-EnvValue -Path $EnvFile -Key 'NGROK_AUTHTOKEN'
    $NgrokAuthtoken = Read-Host 'ngrok authtoken (https://dashboard.ngrok.com/get-started/your-authtoken) [Enter = keep current value]'
    if ([string]::IsNullOrEmpty($NgrokAuthtoken)) { $NgrokAuthtoken = $currentToken }
}

if ([string]::IsNullOrEmpty($NgrokDomain)) {
    $currentDomain = Get-EnvValue -Path $EnvFile -Key 'NGROK_DOMAIN'
    $NgrokDomain = Read-Host 'Reserved ngrok domain, hostname only (e.g. my-store.ngrok-free.app) [Enter = keep current value]'
    if ([string]::IsNullOrEmpty($NgrokDomain)) { $NgrokDomain = $currentDomain }
}
$NgrokDomain = ($NgrokDomain -replace '^https?://', '') -replace '/$', ''
if (Test-Placeholder $NgrokDomain) {
    Write-Warning 'NGROK_DOMAIN still looks like a placeholder - the ngrok container will exit and Chargily cannot reach the payment webhook. Everything else will work.'
}

$secretKeyValue = Get-EnvValue -Path $EnvFile -Key 'SECRET_KEY'
if (Test-Placeholder $secretKeyValue) {
    $secretKeyValue = New-SecretKey
    Write-Host 'SECRET_KEY generated (64 random bytes, base64).'
}
else {
    Write-Host 'SECRET_KEY already present in .env - keeping it.'
}

# -----------------------------------------------------------------------------
# 3. ASP.NET HTTPS certificate (README step 1)
# -----------------------------------------------------------------------------

Write-Step 'Step 1 - ASP.NET HTTPS certificate'

$HttpsDir = Join-Path $env:USERPROFILE '.aspnet\https'
$PfxPath = Join-Path $HttpsDir 'aspnetapp.pfx'

if (-not $SkipTrust -and -not $IsLinux) {
    Invoke-Checked -File 'dotnet' -Arguments @('dev-certs', 'https', '--trust')
}
elseif ($IsLinux) {
    Write-Host 'Skipping "dotnet dev-certs https --trust" - .NET does not support --trust on Linux (the browser will warn until you trust it yourself).'
}

New-Item -ItemType Directory -Force -Path $HttpsDir | Out-Null

if ((Test-Path $PfxPath) -and (Test-PfxPassword -Path $PfxPath -Password $CertificatePassword)) {
    Write-Host "Reusing the existing certificate at $PfxPath (the password matches)."
}
else {
    if (Test-Path $PfxPath) {
        Write-Warning "The existing $PfxPath does not open with the password supplied - regenerating it. (If the stack was already running, its PASSWORD in .env was different.)"
        Remove-Item $PfxPath -Force
    }
    Invoke-Checked -File 'dotnet' -Arguments @('dev-certs', 'https', '-ep', $PfxPath, '-p', $CertificatePassword)
    Write-Host "Certificate exported to $PfxPath."
}

# -----------------------------------------------------------------------------
# 4. Frontend HTTPS certificates (README step 2)
# -----------------------------------------------------------------------------

Write-Step 'Step 2 - Frontend HTTPS certificates (mkcert)'

$FrontendPem = Join-Path $FrontendDir 'localhost+2.pem'
$FrontendPemKey = Join-Path $FrontendDir 'localhost+2-key.pem'

if ((Test-Path $FrontendPem) -and (Test-Path $FrontendPemKey)) {
    Write-Host 'frontend/localhost+2.pem and frontend/localhost+2-key.pem already exist - keeping them.'
}
else {
    # -install can fail for reasons that do not matter here (e.g. mkcert also
    # tries to import its CA into the Java cacerts keystore and lacks write
    # access to it); what this setup needs is the certificate FILES below, so
    # a failing -install only warns. If the browser later reports an untrusted
    # https://localhost:5173, run "mkcert -install" yourself and fix the cause
    # printed there.
    Invoke-Checked -File 'mkcert' -Arguments @('-install') -WarnOnError
    # Three names (localhost, 127.0.0.1, ::1) make mkcert produce exactly the
    # file names compose.yaml mounts: localhost+2.pem / localhost+2-key.pem.
    Invoke-Checked -File 'mkcert' -Arguments @('localhost', '127.0.0.1', '::1') -WorkingDirectory $FrontendDir
    if (-not ((Test-Path $FrontendPem) -and (Test-Path $FrontendPemKey))) {
        throw "mkcert did not produce the expected files (localhost+2.pem, localhost+2-key.pem) in frontend\. compose.yaml mounts those exact names - delete any stray localhost*.pem in frontend\ and re-run this script."
    }
    Write-Host 'Frontend certificates generated in frontend\.'
}

# -----------------------------------------------------------------------------
# 5. Write .env (README step 3)
# -----------------------------------------------------------------------------

Write-Step 'Step 3 - Writing .env'

Set-EnvValue -Path $EnvFile -Key 'PASSWORD' -Value $CertificatePassword
Set-EnvValue -Path $EnvFile -Key 'NGROK_AUTHTOKEN' -Value $NgrokAuthtoken
Set-EnvValue -Path $EnvFile -Key 'NGROK_DOMAIN' -Value $NgrokDomain
Set-EnvValue -Path $EnvFile -Key 'SECRET_KEY' -Value $secretKeyValue
Write-Host 'PASSWORD, NGROK_AUTHTOKEN, NGROK_DOMAIN and SECRET_KEY written to .env.'

# -----------------------------------------------------------------------------
# 6. Start the stack (README step 4)
# -----------------------------------------------------------------------------

if ($SkipStart) {
    Write-Step 'Skipped starting the stack (-SkipStart)'
    Write-Host 'Run it yourself when ready:'
    Write-Host '    docker compose up --build -d'
}
else {
    Write-Step 'Step 4 - Starting the stack (docker compose up --build -d)'
    Write-Host 'The first run takes a few minutes: the API applies its EF Core migrations and seeds the demo catalogue.'
    Invoke-Checked -File 'docker' -Arguments @('compose', 'up', '--build', '-d') -WorkingDirectory $RepoRoot
}

# -----------------------------------------------------------------------------
# 7. Summary (README step 5)
# -----------------------------------------------------------------------------

Write-Step 'Step 5 - Done! Open the application'

Write-Host ''
Write-Host 'Storefront      : https://localhost:5173'
Write-Host 'Swagger UI      : https://localhost:7146/swagger'
Write-Host 'API health      : https://localhost:7146/health'
Write-Host 'Seq (logs)      : http://localhost:5341'
Write-Host 'ngrok inspector : http://localhost:4500'
Write-Host ''
Write-Host 'Demo accounts (seeded database):'
Write-Host '  Admin     demo.admin@gmail.com    / Demo1234!'
Write-Host '  Customer  demo.customer@gmail.com / Demo1234!'
Write-Host ''
Write-Host 'Useful commands:'
Write-Host '  docker compose logs -f api   follow the API container (in-container Serilog writes to Seq, not the console)'
Write-Host '  docker compose down          stop the stack, keeping data'
Write-Host '  docker compose down -v       stop and delete the volumes (the next start re-seeds)'
Write-Host ''
Write-Host 'The frontend container may come up before the API is ready - if https://localhost:5173 errors right after start, wait a few seconds and refresh.'
