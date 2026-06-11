#!/usr/bin/env bash
# ============================================
# run.sh - Run QLDA WebApi (Linux)
# ============================================
#
# Usage: ./run.sh [--dev] [--port]
#   --dev: Use dev schema instead of dbo
#   --port: Expose on port 5000
#
# Examples:
#   ./run.sh           - Run QLDA.WebApi (dbo schema)
#   ./run.sh --dev     - Run QLDA.WebApi (dev schema)
#   ./run.sh --port    - Run QLDA.WebApi on port 5000

set -euo pipefail

# Default to Development environment for local WSL/Linux
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"

WEBAPI_PATH="QLDA.WebApi/QLDA.WebApi.csproj"
URLS_ARG=""

# === Check for --dev flag ===
DEV_SCHEMA=false
for arg in "$@"; do
    if [[ "$arg" == "--dev" ]]; then
        DEV_SCHEMA=true
    fi
done

if [[ "$DEV_SCHEMA" == true ]]; then
    export ConnectionStrings__Schema="dev"
fi

# === Check for --port flag ===
EXPOSE_PORT=false
for arg in "$@"; do
    if [[ "$arg" == "--port" ]]; then
        EXPOSE_PORT=true
    fi
done

if [[ "$EXPOSE_PORT" == true ]]; then
    URLS_ARG="--urls=http://0.0.0.0:5000"
fi

echo ""
echo "============================================"
echo "  QLDA WebApi - Running"
echo "============================================"
echo "  Project: $WEBAPI_PATH"
if [[ "$DEV_SCHEMA" == true ]]; then
    echo "  Schema: dev"
else
    echo "  Schema: dbo (default)"
fi
if [[ "$EXPOSE_PORT" == true ]]; then
    echo "  Port: 5000"
fi
echo "============================================"
echo ""

dotnet watch run $URLS_ARG --project "$WEBAPI_PATH"
