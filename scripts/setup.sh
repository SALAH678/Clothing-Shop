#!/usr/bin/env bash
# =============================================================================
# One-shot setup for the Clothing Shop stack (macOS / Linux).
#
# Automates steps 0-5 of "Running the Application" in README.md:
#   0. Creates .env and frontend/.env from the committed .example files
#      (existing files are never overwritten).
#   1. Generates/trusts the ASP.NET HTTPS development certificate and exports
#      it to ~/.aspnet/https/aspnetapp.pfx.
#   2. Generates the frontend HTTPS certificates with mkcert
#      (frontend/localhost+2.pem and frontend/localhost+2-key.pem).
#   3. Fills PASSWORD, NGROK_AUTHTOKEN, NGROK_DOMAIN and SECRET_KEY into
#      .env. Only the values you can supply are prompted for; SECRET_KEY is
#      generated automatically.
#   4. Starts the stack with "docker compose up --build -d".
#   5. Prints the URLs of every service and the demo accounts.
#
# The script is idempotent: at every prompt, pressing Enter keeps the value
# already stored in .env, and files that already exist are reused.
#
# Usage:
#   ./scripts/setup.sh                interactive setup
#   ./scripts/setup.sh --skip-start   everything except "docker compose up"
#   ./scripts/setup.sh --skip-trust   skip "dotnet dev-certs https --trust"
# =============================================================================
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"
FRONTEND_DIR="$REPO_ROOT/frontend"

SKIP_START=0
SKIP_TRUST=0
for arg in "$@"; do
  case "$arg" in
    --skip-start) SKIP_START=1 ;;
    --skip-trust) SKIP_TRUST=1 ;;
    -h|--help) sed -n '2,25p' "$0"; exit 0 ;;
    *) echo "Unknown option: $arg (expected --skip-start, --skip-trust or --help)" >&2; exit 1 ;;
  esac
done

# -----------------------------------------------------------------------------
# Helpers
# -----------------------------------------------------------------------------

step() {
  printf '\n%s\n==> %s\n%s\n' \
    "======================================================================" "$1" \
    "======================================================================"
}

fail() { echo "ERROR: $1" >&2; exit 1; }

require_cmd() { command -v "$1" >/dev/null 2>&1 || fail "'$1' was not found on PATH. $2"; }

run_checked() {
  echo "+ $*"
  "$@" || fail "'$*' failed with exit code $?."
}

get_env_value() { sed -n "s/^$2=//p" "$1" | head -n 1; }

set_env_value() {
  local file="$1" key="$2" value="$3" found=0 line tmp
  tmp="$(mktemp)"
  while IFS= read -r line || [ -n "$line" ]; do
    case "$line" in
      "$key="*) printf '%s=%s\n' "$key" "$value" >>"$tmp"; found=1 ;;
      *) printf '%s\n' "$line" >>"$tmp" ;;
    esac
  done <"$file"
  [ "$found" -eq 1 ] || printf '%s=%s\n' "$key" "$value" >>"$tmp"
  mv "$tmp" "$file"
}

is_placeholder() {
  case "$1" in
    ""|choose-*|replace-*|your-*) return 0 ;;
    *) return 1 ;;
  esac
}

new_secret_key() {
  if command -v openssl >/dev/null 2>&1; then
    openssl rand -base64 64 | tr -d '\n'
  else
    head -c 64 /dev/urandom | base64 | tr -d '\n'
  fi
}

pfx_opens_with() {
  local pfx="$1" pass="$2"
  command -v openssl >/dev/null 2>&1 || return 1
  # Newer openssl needs -legacy for the 3DES/SHA1 bags dotnet exports;
  # macOS LibreSSL rejects the -legacy flag, so try without it first.
  openssl pkcs12 -in "$pfx" -passin "pass:$pass" -noout >/dev/null 2>&1 \
    || openssl pkcs12 -in "$pfx" -passin "pass:$pass" -noout -legacy >/dev/null 2>&1
}

prompt_keep() {
  local prompt="$1" current="$2" answer=""
  read -r -p "$prompt [Enter = keep current value]: " answer || true
  printf '%s' "${answer:-$current}"
}

prompt_keep_secret() {
  local prompt="$1" current="$2" answer=""
  read -r -s -p "$prompt [Enter = keep current value]: " answer || true
  echo >&2
  printf '%s' "${answer:-$current}"
}

# -----------------------------------------------------------------------------
# 0. Prerequisites
# -----------------------------------------------------------------------------

step 'Checking prerequisites'

require_cmd dotnet 'Install the .NET 10 SDK: https://dotnet.microsoft.com/download'
require_cmd mkcert 'Install mkcert: https://github.com/FiloSottile/mkcert'
require_cmd docker 'Install Docker Engine / Docker Desktop: https://docs.docker.com/engine/install/'
docker compose version >/dev/null 2>&1 \
  || fail "'docker compose' (Compose v2) is not available. Update Docker or install the Compose v2 plugin."
if ! docker info >/dev/null 2>&1; then
  if [ "$SKIP_START" -eq 1 ]; then
    echo "WARNING: Docker is not running - continuing because --skip-start was supplied, but 'docker compose up' will need the daemon later." >&2
  else
    fail 'Docker is not running. Start Docker (or the Docker Desktop app) and run this script again.'
  fi
fi
echo 'All prerequisites found (dotnet, mkcert, docker + compose v2).'

# compose.yaml mounts ${USERPROFILE}/.aspnet/https on every OS. Export it for
# the duration of this run (the PowerShell/README portability note) so
# "docker compose up" below works unchanged. Add it to ~/.zshrc or ~/.bashrc
# to run docker compose commands yourself later.
export USERPROFILE="${USERPROFILE:-$HOME}"

# -----------------------------------------------------------------------------
# 1. Environment files (README step 0)
# -----------------------------------------------------------------------------

step 'Step 0 - Creating the environment files'

if [ -f "$ENV_FILE" ]; then
  echo '.env already exists - keeping it.'
else
  cp "$REPO_ROOT/.env.example" "$ENV_FILE"
  echo '.env created from .env.example.'
fi

if [ -f "$FRONTEND_DIR/.env" ]; then
  echo 'frontend/.env already exists - keeping it.'
else
  cp "$FRONTEND_DIR/.env.example" "$FRONTEND_DIR/.env"
  echo 'frontend/.env created from frontend/.env.example.'
fi

# -----------------------------------------------------------------------------
# 2. Collect the values only you can supply (README step 3)
# -----------------------------------------------------------------------------

step 'Step 3 - Collecting configuration values'

CERTIFICATE_PASSWORD="$(prompt_keep_secret 'Certificate password (becomes PASSWORD in .env)' "$(get_env_value "$ENV_FILE" PASSWORD)")"
if is_placeholder "$CERTIFICATE_PASSWORD"; then
  fail 'A certificate password is required - it must match the password used to generate the .pfx below.'
fi

NGROK_AUTHTOKEN="$(prompt_keep 'ngrok authtoken (https://dashboard.ngrok.com/get-started/your-authtoken)' "$(get_env_value "$ENV_FILE" NGROK_AUTHTOKEN)")"

NGROK_DOMAIN="$(prompt_keep 'Reserved ngrok domain, hostname only (e.g. my-store.ngrok-free.app)' "$(get_env_value "$ENV_FILE" NGROK_DOMAIN)")"
NGROK_DOMAIN="${NGROK_DOMAIN#https://}"
NGROK_DOMAIN="${NGROK_DOMAIN%/}"
if is_placeholder "$NGROK_DOMAIN"; then
  echo "WARNING: NGROK_DOMAIN still looks like a placeholder - the ngrok container will exit and Chargily cannot reach the payment webhook. Everything else will work." >&2
fi

SECRET_KEY_VALUE="$(get_env_value "$ENV_FILE" SECRET_KEY)"
if is_placeholder "$SECRET_KEY_VALUE"; then
  SECRET_KEY_VALUE="$(new_secret_key)"
  echo 'SECRET_KEY generated (64 random bytes, base64).'
else
  echo 'SECRET_KEY already present in .env - keeping it.'
fi

# -----------------------------------------------------------------------------
# 3. ASP.NET HTTPS certificate (README step 1)
# -----------------------------------------------------------------------------

step 'Step 1 - ASP.NET HTTPS certificate'

HTTPS_DIR="$HOME/.aspnet/https"
PFX_PATH="$HTTPS_DIR/aspnetapp.pfx"

if [ "$SKIP_TRUST" -eq 1 ]; then
  echo 'Skipping "dotnet dev-certs https --trust" (--skip-trust).'
elif [ "$(uname)" = 'Linux' ]; then
  echo 'Skipping "dotnet dev-certs https --trust" - .NET does not support --trust on Linux (the browser will warn until you trust it yourself).'
else
  run_checked dotnet dev-certs https --trust
fi

mkdir -p "$HTTPS_DIR"

if [ -f "$PFX_PATH" ] && pfx_opens_with "$PFX_PATH" "$CERTIFICATE_PASSWORD"; then
  echo "Reusing the existing certificate at $PFX_PATH (the password matches)."
else
  if [ -f "$PFX_PATH" ]; then
    echo "WARNING: the existing $PFX_PATH does not open with the password supplied - regenerating it. (If the stack was already running, its PASSWORD in .env was different.)" >&2
    rm -f "$PFX_PATH"
  fi
  run_checked dotnet dev-certs https -ep "$PFX_PATH" -p "$CERTIFICATE_PASSWORD"
  echo "Certificate exported to $PFX_PATH."
fi

# -----------------------------------------------------------------------------
# 4. Frontend HTTPS certificates (README step 2)
# -----------------------------------------------------------------------------

step 'Step 2 - Frontend HTTPS certificates (mkcert)'

FRONTEND_PEM="$FRONTEND_DIR/localhost+2.pem"
FRONTEND_PEM_KEY="$FRONTEND_DIR/localhost+2-key.pem"

if [ -f "$FRONTEND_PEM" ] && [ -f "$FRONTEND_PEM_KEY" ]; then
  echo 'frontend/localhost+2.pem and frontend/localhost+2-key.pem already exist - keeping them.'
else
  # -install can fail for reasons that do not matter here (e.g. mkcert also
  # tries to import its CA into the Java cacerts keystore and lacks write
  # access to it); what this setup needs is the certificate FILES below, so a
  # failing -install only warns. If the browser later reports an untrusted
  # https://localhost:5173, run "mkcert -install" yourself and fix the cause
  # printed there.
  if ! mkcert -install; then
    echo "WARNING: 'mkcert -install' returned a non-zero exit code - continuing, because only the certificate files are needed here." >&2
  fi
  # Three names (localhost, 127.0.0.1, ::1) make mkcert produce exactly the
  # file names compose.yaml mounts: localhost+2.pem / localhost+2-key.pem.
  (cd "$FRONTEND_DIR" && run_checked mkcert localhost 127.0.0.1 '::1')
  if [ ! -f "$FRONTEND_PEM" ] || [ ! -f "$FRONTEND_PEM_KEY" ]; then
    fail 'mkcert did not produce the expected files (localhost+2.pem, localhost+2-key.pem) in frontend/. compose.yaml mounts those exact names - delete any stray localhost*.pem in frontend/ and re-run this script.'
  fi
  echo 'Frontend certificates generated in frontend/.'
fi

# -----------------------------------------------------------------------------
# 5. Write .env (README step 3)
# -----------------------------------------------------------------------------

step 'Step 3 - Writing .env'

set_env_value "$ENV_FILE" PASSWORD "$CERTIFICATE_PASSWORD"
set_env_value "$ENV_FILE" NGROK_AUTHTOKEN "$NGROK_AUTHTOKEN"
set_env_value "$ENV_FILE" NGROK_DOMAIN "$NGROK_DOMAIN"
set_env_value "$ENV_FILE" SECRET_KEY "$SECRET_KEY_VALUE"
echo 'PASSWORD, NGROK_AUTHTOKEN, NGROK_DOMAIN and SECRET_KEY written to .env.'

# -----------------------------------------------------------------------------
# 6. Start the stack (README step 4)
# -----------------------------------------------------------------------------

if [ "$SKIP_START" -eq 1 ]; then
  step 'Skipped starting the stack (--skip-start)'
  echo 'Run it yourself when ready:'
  echo '    docker compose up --build -d'
else
  step 'Step 4 - Starting the stack (docker compose up --build -d)'
  echo 'The first run takes a few minutes: the API applies its EF Core migrations and seeds the demo catalogue.'
  (cd "$REPO_ROOT" && run_checked docker compose up --build -d)
fi

# -----------------------------------------------------------------------------
# 7. Summary (README step 5)
# -----------------------------------------------------------------------------

step 'Step 5 - Done! Open the application'

echo ''
echo 'Storefront      : https://localhost:5173'
echo 'Swagger UI      : https://localhost:7146/swagger'
echo 'API health      : https://localhost:7146/health'
echo 'Seq (logs)      : http://localhost:5341'
echo 'ngrok inspector : http://localhost:4500'
echo ''
echo 'Demo accounts (seeded database):'
echo '  Admin     demo.admin@gmail.com    / Demo1234!'
echo '  Customer  demo.customer@gmail.com / Demo1234!'
echo ''
echo 'Useful commands:'
echo '  docker compose down      stop the stack, keeping data'
echo '  docker compose down -v   stop and delete the volumes (the next start re-seeds)'
echo ''
echo 'The frontend container may come up before the API is ready - if https://localhost:5173 errors right after start, wait a few seconds and refresh.'
