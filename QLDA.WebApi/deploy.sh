#!/usr/bin/env bash
# ============================================
# deploy.sh - QLDA Module Deployment (Linux/WSL)
# ============================================
#
# Usage: ./deploy.sh [Environment] [DeploymentMode]
#   Environment: Staging (default), Production
#   DeploymentMode: Incremental (default), Full
#
# Examples:
#   ./deploy.sh                     - Staging + Incremental
#   ./deploy.sh Staging             - Staging + Incremental
#   ./deploy.sh Staging Full        - Staging + Full deployment
#   ./deploy.sh Production          - Production build only (no deploy)
#
# Destinations:
#   Staging:    //192.168.1.12/API_MND/TTCDS/QuanLyDuAn
#   Production: Build only (no server deploy)
# ============================================

set -euo pipefail

# === Destination Base Path ===
DESTINATION_UNC="//192.168.1.12/API_MND/TTCDS"

# === Load .env file if exists ===
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ -f "$SCRIPT_DIR/.env" ]]; then
    echo "Loading environment from .env file..."
    set -a
    source "$SCRIPT_DIR/.env"
    set +a
fi

# === Parse Arguments ===
ENVIRONMENT="${1:-Staging}"
DEPLOYMENT_MODE="${2:-Incremental}"

# === Validate Environment ===
if [[ ! "$ENVIRONMENT" =~ ^(Staging|Production)$ ]]; then
    echo ""
    echo "ERROR: Invalid environment \"$ENVIRONMENT\""
    echo ""
    echo "Usage: $0 [Environment] [DeploymentMode]"
    echo "  Environment: Staging (default), Production"
    echo "  DeploymentMode: Incremental (default), Full"
    echo ""
    exit 1
fi

# === Validate DeploymentMode ===
if [[ ! "$DEPLOYMENT_MODE" =~ ^(Incremental|Full)$ ]]; then
    echo ""
    echo "ERROR: Invalid deployment mode \"$DEPLOYMENT_MODE\""
    echo ""
    echo "Usage: $0 [Environment] [DeploymentMode]"
    echo "  DeploymentMode: Incremental (default), Full"
    echo ""
    exit 1
fi

# === Set Destination Path ===
DESTINATION_PATH="${DESTINATION_UNC}/QuanLyDuAn"

# === Adjust Production Destination ===
if [[ "$ENVIRONMENT" == "Production" ]]; then
    DESTINATION_PATH="${DESTINATION_PATH}/Production"
fi

# === Display Deployment Info ===
echo ""
echo "============================================"
echo "  QLDA MODULE DEPLOYMENT"
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
echo "Publishing QLDA.WebApi..."
PUBLISH_PATH="$SCRIPT_DIR/bin/Release/net8.0/publish"

mkdir -p "$SCRIPT_DIR/bin/Release/net8.0"

if ! dotnet publish "$SCRIPT_DIR/QLDA.WebApi.csproj" --configuration Release --output "$PUBLISH_PATH"; then
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
rm -f "${PUBLISH_PATH}"/QLDA.*.Tests.* 2>/dev/null || true
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

# === Deploy ===
if [[ "$ENVIRONMENT" == "Production" ]]; then
    echo ""
    echo "============================================"
    echo "  QLDA PRODUCTION BUILD COMPLETE"
    echo "============================================"
    echo "  Build artifacts: ${PUBLISH_PATH}"
    echo "============================================"
else
    echo "Copying files to server..."

    # SMB server configuration
    SMB_SERVER="${SMB_SERVER:-192.168.1.12}"
    SMB_SHARE="${SMB_SHARE:-API_MND}"
    SMB_USER="${SMB_USER:-update}"
    SMB_PASS="${SMB_PASS:-Vietinfo@#@!}"
    SMB_TARGET="${SMB_TARGET:-TTCDS/QuanLyDuAn}"

    if command -v smbclient &> /dev/null; then
        echo "Using smbclient for file transfer..."
        echo "Server: $SMB_SERVER, Share: $SMB_SHARE, Target: $SMB_TARGET"

        # === Use app_offline.htm to take down site ===
        echo "Checking for app_offline.htm on remote..."
        if smbclient "//$SMB_SERVER/$SMB_SHARE" --directory="$SMB_TARGET" -U "$SMB_USER%$SMB_PASS" -c "ls app_offline.htm" 2>/dev/null | grep -q "app_offline.htm"; then
            echo "app_offline.htm already exists, site should be down."
        else
            echo "Creating app_offline.htm to take site offline..."
            echo "<html><body><h1>Deploying...</h1></body></html>" > /tmp/app_offline.htm
            smbclient "//$SMB_SERVER/$SMB_SHARE" --directory="$SMB_TARGET" -U "$SMB_USER%$SMB_PASS" -c "put /tmp/app_offline.htm app_offline.htm" 2>/dev/null || true
            echo "Waiting for site to go down..."
            sleep 3
        fi

        # Delete locked files first
        echo "Removing old files from destination..."
        smbclient "//$SMB_SERVER/$SMB_SHARE" --directory="$SMB_TARGET" -U "$SMB_USER%$SMB_PASS" -c "prompt OFF; recurse ON; del QLDA.WebApi*; del *.dll; del *.pdb; del *.xml" 2>/dev/null || true
        sleep 1

        # Upload all files
        echo "Uploading new files..."
        smbclient "//$SMB_SERVER/$SMB_SHARE" --directory="$SMB_TARGET" -U "$SMB_USER%$SMB_PASS" -c "prompt OFF; recurse ON; lcd \"$PUBLISH_PATH\"; mput *" 2>/dev/null
        COPY_SUCCESS=$?

        # === Bring site back online ===
        echo "Renaming app_offline.htm to app_offline_.htm..."
        smbclient "//$SMB_SERVER/$SMB_SHARE" --directory="$SMB_TARGET" -U "$SMB_USER%$SMB_PASS" -c "rename app_offline.htm app_offline_.htm" 2>/dev/null || true
    else
        if [[ -d "$DESTINATION_PATH" ]] || mountpoint -q "$(dirname "$DESTINATION_PATH")" 2>/dev/null; then
            echo "Using rsync for file transfer..."
            rsync -av --delete "$PUBLISH_PATH/" "$DESTINATION_PATH/"
            COPY_SUCCESS=$?
        else
            echo "Warning: Neither smbclient nor rsync available."
            echo "Please mount the network share manually and copy from:"
            echo "  Source: $PUBLISH_PATH"
            echo "  Destination: $DESTINATION_PATH"
            echo ""
            echo "Example mount command:"
            echo "  sudo mount -t cifs //192.168.1.12/API_MND/TTCDS/QuanLyDuAn /mnt/deploy -o username=YOUR_USER"
            COPY_SUCCESS=1
        fi
    fi

    if [[ $COPY_SUCCESS -eq 0 || -z $COPY_SUCCESS ]]; then
        echo ""
        echo "============================================"
        echo "  QLDA DEPLOYMENT SUCCESSFUL"
        echo "============================================"
        echo "  Destination: ${DESTINATION_PATH}"
        echo "============================================"
    else
        echo "Deployment failed."
        exit 1
    fi
fi
