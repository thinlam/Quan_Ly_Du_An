#!/usr/bin/env bash
# ============================================
# deploy.sh - Unified Module Deployment (Linux/WSL)
# ============================================
#
# Usage: ./deploy.sh <Module> [Environment] [DeploymentMode]
#   Module: DVDC, QLDA, NVTT, QLHD (required)
#   Environment: Staging (default), Production
#   DeploymentMode: Incremental (default), Full
#
# Examples:
#   ./deploy.sh DVDC                    - DVDC to Staging
#   ./deploy.sh QLHD Staging Full       - QLHD Full deployment
#   ./deploy.sh NVTT Production         - NVTT Production build
#
# Destinations (UNC via Windows host network):
#   DVDC: \\192.168.1.12\api_mnd\TTCDS\DichVuDungChung
#   QLDA: \\192.168.1.12\api_mnd\TTCDS\QuanLyDuAn
#   NVTT: \\192.168.1.12\api_mnd\TTCDS\NhiemVuTrongTam
#   QLHD: \\192.168.1.12\api_mnd\TTCDS\QuanLyHopDong
# ============================================

set -euo pipefail

# === Destination Base Path ===
# UNC path for Windows (used via powershell.exe from WSL)
DESTINATION_UNC="\\\\192.168.1.12\\api_mnd\\TTCDS"

# === Parse Arguments ===
MODULE="${1:-}"
ENVIRONMENT="${2:-Staging}"
DEPLOYMENT_MODE="${3:-Incremental}"

# === Validate Module (Required) ===
if [[ -z "$MODULE" ]]; then
    echo ""
    echo "ERROR: Module parameter is required"
    echo ""
    echo "Usage: ./deploy.sh <Module> [Environment] [DeploymentMode]"
    echo "  Module: DVDC, QLDA, NVTT, QLHD"
    echo "  Environment: Staging (default), Production"
    echo "  DeploymentMode: Incremental (default), Full"
    echo ""
    exit 1
fi

# === Validate and Set Module Configuration (case-insensitive) ===
case "${MODULE,,}" in
    dvdc)
        MODULE="DVDC"
        MODULE_NAME="DichVuDungChung"
        WEBAPI_PATH="../DichVuDungChung/SER/DVDC.WebApi/DVDC.WebApi.csproj"
        DESTINATION_PATH="${DESTINATION_UNC}\\DichVuDungChung"
        ;;
    qlda)
        MODULE="QLDA"
        MODULE_NAME="QuanLyDuAn"
        WEBAPI_PATH="QLDA.WebApi/QLDA.WebApi.csproj"
        DESTINATION_PATH="${DESTINATION_UNC}\\QuanLyDuAn"
        ;;
    nvtt)
        MODULE="NVTT"
        MODULE_NAME="NhiemVuTrongTam"
        WEBAPI_PATH="../NhiemVuTrongTam/SER/NVTT.WebApi/NVTT.WebApi.csproj"
        DESTINATION_PATH="${DESTINATION_UNC}\\NhiemVuTrongTam"
        ;;
    qlhd)
        MODULE="QLHD"
        MODULE_NAME="QuanLyHopDong"
        WEBAPI_PATH="modules/QLHD/QLHD.WebApi/QLHD.WebApi.csproj"
        DESTINATION_PATH="${DESTINATION_UNC}\\QuanLyHopDong"
        ;;
    *)
        echo ""
        echo "ERROR: Invalid module \"$MODULE\""
        echo ""
        echo "Valid modules: DVDC, QLDA, NVTT, QLHD"
        echo ""
        exit 1
        ;;
esac

# === Validate Environment ===
if [[ ! "$ENVIRONMENT" =~ ^(Staging|Production)$ ]]; then
    echo ""
    echo "ERROR: Invalid environment \"$ENVIRONMENT\""
    echo ""
    exit 1
fi

# === Validate DeploymentMode ===
if [[ ! "$DEPLOYMENT_MODE" =~ ^(Incremental|Full)$ ]]; then
    echo ""
    echo "ERROR: Invalid deployment mode \"$DEPLOYMENT_MODE\""
    echo ""
    exit 1
fi

# === Adjust Production Destination ===
if [[ "$ENVIRONMENT" == "Production" ]]; then
    DESTINATION_PATH="${DESTINATION_PATH}/Production"
fi

# === Display Deployment Info ===
echo ""
echo "============================================"
echo "  ${MODULE} (${MODULE_NAME}) DEPLOYMENT"
echo "============================================"
echo "  Environment    : ${ENVIRONMENT}"
echo "  Deployment Mode: ${DEPLOYMENT_MODE}"
if [[ "$ENVIRONMENT" == "Production" ]]; then
    echo "  Destination    : Build Only (no server deploy)"
else
    echo "  Destination    : ${DESTINATION_PATH}"
fi
echo "============================================"
echo ""

# === Build Project ===
echo "Publishing ${MODULE}.WebApi..."
PUBLISH_PATH="$(pwd)/bin/Release/net8.0/publish/${MODULE}"

mkdir -p "./bin/Release/net8.0/publish"

if ! dotnet publish "$WEBAPI_PATH" --configuration Release --output "$PUBLISH_PATH"; then
    echo "Publish failed."
    exit 1
fi

# === Cleanup Publish Folder ===
echo "Cleaning up publish folder..."

# Remove dev config
[[ -f "$PUBLISH_PATH/appsettings.Development.json" ]] && rm -f "$PUBLISH_PATH/appsettings.Development.json"

# Remove env-specific configs not matching target
if [[ "$ENVIRONMENT" == "Staging" ]]; then
    rm -f "$PUBLISH_PATH/appsettings.Production.json"
fi
if [[ "$ENVIRONMENT" == "Production" ]]; then
    rm -f "$PUBLISH_PATH/appsettings.Staging.json"
fi

# Remove sensitive and non-deployable files
rm -f "$PUBLISH_PATH"/.env* 2>/dev/null || true
rm -f "$PUBLISH_PATH"/*.md 2>/dev/null || true

for f in Deploy.bat deploy.bat deploy.sh DeployScript.ps1 makefile Dockerfile .dockerignore; do
    rm -f "${PUBLISH_PATH}/${f}"
done

# Remove non-deployable directories
for d in .claude plans docs logs Tests backup; do
    rm -rf "${PUBLISH_PATH}/${d}"
done

# Remove test-related files
rm -f "${PUBLISH_PATH}/${MODULE}".*.Tests.* 2>/dev/null || true
rm -f "${PUBLISH_PATH}"/*Tests*.dll 2>/dev/null || true
rm -f "${PUBLISH_PATH}"/*Tests*.json 2>/dev/null || true
rm -f "${PUBLISH_PATH}"/Moq.dll 2>/dev/null || true
rm -f "${PUBLISH_PATH}"/xunit*.dll 2>/dev/null || true
rm -f "${PUBLISH_PATH}"/coverlet*.dll 2>/dev/null || true
rm -f "${PUBLISH_PATH}"/Microsoft.NET.Test*.dll 2>/dev/null || true

# Remove non-English localization folders
echo "Removing non-English localization folders..."
find "$PUBLISH_PATH" -maxdepth 1 -type d -regex '.*/[a-z]\{2\}\(-[A-Za-z]\+\)\?$' \
    ! -name 'en' ! -name 'en-US' ! -name 'en-GB' -exec rm -rf {} + 2>/dev/null || true

echo "Cleanup completed."

# === Update web.config ===
if [[ -f "$PUBLISH_PATH/web.config" ]]; then
    echo "Setting ASPNETCORE_ENVIRONMENT to ${ENVIRONMENT}..."
    sed -i "s|name=\"ASPNETCORE_ENVIRONMENT\" value=\"[^\"]*\"|name=\"ASPNETCORE_ENVIRONMENT\" value=\"${ENVIRONMENT}\"|g" \
        "${PUBLISH_PATH}/web.config"
    echo "web.config updated"
fi

# === Load credentials from .env ===
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="${SCRIPT_DIR}/.env"

if [[ -f "$ENV_FILE" ]]; then
    set -a
    source "$ENV_FILE"
    set +a
fi

# SMB credentials (load from .env)
SMB_USERNAME="${SMB_USERNAME:-}"
SMB_PASSWORD="${SMB_PASSWORD:-}"

# === Detect available deployment method ===
# Check if wslpath is available (WSL environment)
USE_WSL=false
if command -v wslpath &> /dev/null; then
    USE_WSL=true
    echo "WSL detected - using wslpath + PowerShell deployment"
elif command -v smbclient &> /dev/null; then
    USE_SMB=true
    echo "smbclient detected - using SMB deployment"
else
    echo "Warning: Neither wslpath nor smbclient found. Deployment may fail."
    USE_SMB=false
    USE_WSL=false
fi

# === Deploy ===
if [[ "$ENVIRONMENT" == "Production" ]]; then
    echo ""
    echo "============================================"
    echo "  ${MODULE} PRODUCTION BUILD COMPLETE"
    echo "============================================"
    echo "  Build artifacts: ${PUBLISH_PATH}"
    echo "============================================"
else
    echo "Copying files to server..."

    if [[ "$USE_WSL" == "true" ]]; then
        # Convert WSL publish path to Windows path for powershell.exe
        WIN_PUBLISH_PATH=$(wslpath -w "$PUBLISH_PATH")

        # Use PowerShell via Windows host (has VPN/network access)
        SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
        WIN_SCRIPT_DIR=$(wslpath -w "${SCRIPT_DIR}/QLDA.WebApi")

        if [[ "$DEPLOYMENT_MODE" == "Incremental" ]]; then
            echo "Incremental mode: excluding web.config and PrintTemplates"
            powershell.exe -ExecutionPolicy Bypass -File "${WIN_SCRIPT_DIR}\\DeployScript.ps1" \
                -SourcePath "$WIN_PUBLISH_PATH" \
                -DestinationPath "$DESTINATION_PATH" \
                -DeploymentMode Incremental
        else
            echo "Full mode: including web.config and PrintTemplates"
            powershell.exe -ExecutionPolicy Bypass -File "${WIN_SCRIPT_DIR}\\DeployScript.ps1" \
                -SourcePath "$WIN_PUBLISH_PATH" \
                -DestinationPath "$DESTINATION_PATH" \
                -DeploymentMode Full
        fi
    elif [[ "$USE_SMB" == "true" ]]; then
        # Use smbclient for Linux native deployment
        SMB_SERVER="192.168.1.12"
        SMB_SHARE="api_mnd"

        # Get destination folder from env (e.g., SMB_DEST_QLHD=\\192.168.1.12\api_mnd\TTCDS\QuanLyHopDong)
        ENV_VAR="SMB_DEST_${MODULE}"
        FULL_DEST="${!ENV_VAR:-}"
        # Extract share-relative path (e.g., "\\192.168.1.12\api_mnd\TTCDS\QuanLyHopDong" -> "TTCDS/QuanLyHopDong")
        UNIX_PATH=$(echo "$FULL_DEST" | tr '\\' '/')
        SERVER_PREFIX="//${SMB_SERVER}/"
        SHARE_RELATIVE_PATH="${UNIX_PATH#$SERVER_PREFIX}"
        SHARE_RELATIVE_PATH="${SHARE_RELATIVE_PATH#$SMB_SHARE/}"

        # Build smbclient command with credentials
        SMB_BASE_CMD="smbclient \"//${SMB_SERVER}/${SMB_SHARE}\""
        if [[ -n "$SMB_USERNAME" && -n "$SMB_PASSWORD" ]]; then
            SMB_BASE_CMD+=" -U \"${SMB_USERNAME}%${SMB_PASSWORD}\""
        elif [[ -f ~/.smbcredentials ]]; then
            SMB_BASE_CMD+=" -A ~/.smbcredentials"
        else
            SMB_BASE_CMD+=" -N"
        fi

        # === Step 1: Take app offline ===
        echo "Taking app offline..."
        if eval "$SMB_BASE_CMD -c \"cd $SHARE_RELATIVE_PATH ; ls app_offline_.htm\"" 2>&1 | grep -q "app_offline_.htm"; then
            echo "Found app_offline_.htm - renaming to app_offline.htm..."
            eval "$SMB_BASE_CMD -c \"cd $SHARE_RELATIVE_PATH ; rename app_offline_.htm app_offline.htm\"" 2>/dev/null || true
        else
            echo "Creating app_offline.htm..."
            echo "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Server Offline</title></head><body><h1>Server Already Offline</h1><p>The application is currently being updated. Please try again in a few moments.</p></body></html>" > /tmp/app_offline.htm
            eval "$SMB_BASE_CMD -c \"cd $SHARE_RELATIVE_PATH ; put /tmp/app_offline.htm app_offline.htm\"" 2>/dev/null || true
            rm -f /tmp/app_offline.htm
        fi

        # Wait for IIS to unload
        echo "Waiting for service to stop..."
        sleep 3

        # === Step 2: Deploy files ===
        DEPLOY_STATUS=0
        if [[ "$DEPLOYMENT_MODE" == "Incremental" ]]; then
            echo "Incremental mode: copying today-modified files..."
            TODAY_FILES=$(find "$PUBLISH_PATH" -type f -newermt "$(date +%Y-%m-%d)" ! -name "web.config" ! -path "*PrintTemplates*" 2>/dev/null)
            if [[ -z "$TODAY_FILES" ]]; then
                echo "No files modified today to deploy."
            else
                while IFS= read -r file; do
                    rel_path="${file#$PUBLISH_PATH/}"
                    dir="${rel_path%/*}"
                    filename="${rel_path##*/}"

                    if [[ "$dir" != "$rel_path" ]]; then
                        # Create dir and upload
                        eval "$SMB_BASE_CMD -c \"cd $SHARE_RELATIVE_PATH ; mkdir \\\"$dir\\\" 2>/dev/null ; cd \\\"$dir\\\" ; put \\\"$file\\\" \\\"$filename\\\"\"" 2>/dev/null || DEPLOY_STATUS=1
                    else
                        eval "$SMB_BASE_CMD -c \"cd $SHARE_RELATIVE_PATH ; put \\\"$file\\\" \\\"$filename\\\"\"" 2>/dev/null || DEPLOY_STATUS=1
                    fi
                done <<< "$TODAY_FILES"
            fi
        else
            echo "Full mode: syncing all files..."
            eval "$SMB_BASE_CMD -c \"lcd '$PUBLISH_PATH' ; cd $SHARE_RELATIVE_PATH ; prompt OFF ; recurse ON ; mput *\"" 2>/dev/null || DEPLOY_STATUS=1
        fi

        # === Step 3: Bring app back online ===
        echo "Bringing app back online..."
        eval "$SMB_BASE_CMD -c \"cd $SHARE_RELATIVE_PATH ; rename app_offline.htm app_offline_.htm\"" 2>/dev/null || true
        sleep 2
    else
        echo "Error: No suitable deployment method available (wslpath or smbclient required)"
        echo "Please install either:"
        echo "  - wslu (for WSL): apt install wslu"
        echo "  - smbclient (for Linux): apt install smbclient"
        exit 1
    fi

    if [[ "$DEPLOY_STATUS" == "0" ]]; then
        echo ""
        echo "============================================"
        echo "  ${MODULE} DEPLOYMENT SUCCESSFUL"
        echo "============================================"
        echo "  Destination: ${DESTINATION_PATH}"
        echo "============================================"
    else
        echo "Deployment failed."
        exit 1
    fi
fi
