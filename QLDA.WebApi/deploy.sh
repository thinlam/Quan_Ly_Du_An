#!/bin/bash
# ============================================
# deploy.sh - Environment-Based Deployment
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
#   Staging:    //App01/API_MND/TTCDS/QuanLyDuAn
#   Production: Build only (no server deploy)
# ============================================

set -e

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

# === Set Destination Path Based on Environment ===
case "$ENVIRONMENT" in
    Staging|staging)
        DESTINATION_PATH="//App01/API_MND/TTCDS/QuanLyDuAn"
        ;;
    Production|production)
        DESTINATION_PATH=""
        ;;
    *)
        echo ""
        echo "ERROR: Invalid environment \"$ENVIRONMENT\""
        echo ""
        echo "Usage: $0 [Environment] [DeploymentMode]"
        echo "  Environment: Staging, Production"
        echo "  DeploymentMode: Incremental, Full"
        echo ""
        exit 1
        ;;
esac

# === Validate DeploymentMode ===
case "$DEPLOYMENT_MODE" in
    Incremental|incremental|Full|full)
        ;;
    *)
        echo ""
        echo "ERROR: Invalid deployment mode \"$DEPLOYMENT_MODE\""
        echo ""
        echo "Usage: $0 [Environment] [DeploymentMode]"
        echo "  DeploymentMode: Incremental, Full"
        echo ""
        exit 1
        ;;
esac

# === Display Deployment Info ===
echo ""
echo "============================================"
echo "  DEPLOYMENT CONFIGURATION"
echo "============================================"
echo "  Environment    : $ENVIRONMENT"
echo "  Deployment Mode: $DEPLOYMENT_MODE"
if [[ "$ENVIRONMENT" == "Production" || "$ENVIRONMENT" == "production" ]]; then
    echo "  Destination    : Build Only (no server deploy)"
else
    echo "  Destination    : $DESTINATION_PATH"
fi
echo "============================================"
echo ""

# === Build Project ===
echo "Publishing project..."

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PUBLISH_PATH="$SCRIPT_DIR/bin/Release/net8.0/publish"

dotnet publish "$SCRIPT_DIR/QLDA.WebApi.csproj" --configuration Release --output "$PUBLISH_PATH"

if [[ $? -ne 0 ]]; then
    echo "Publish failed with error code $?."
    exit $?
fi

# === Cleanup Publish Folder ===
echo "Cleaning up publish folder..."

# Remove development config (always)
rm -f "$PUBLISH_PATH/appsettings.Development.json"

# Remove config based on target environment
if [[ "$ENVIRONMENT" == "Staging" || "$ENVIRONMENT" == "staging" ]]; then
    # Deploying to Staging: remove Production config, keep Staging
    rm -f "$PUBLISH_PATH/appsettings.Production.json"
fi
if [[ "$ENVIRONMENT" == "Production" || "$ENVIRONMENT" == "production" ]]; then
    # Deploying to Production: remove Staging config, keep Production
    rm -f "$PUBLISH_PATH/appsettings.Staging.json"
fi

# Remove env files
rm -f "$PUBLISH_PATH/.env"*

# Remove markdown files
rm -f "$PUBLISH_PATH"/*.md

# Remove deployment scripts
rm -f "$PUBLISH_PATH/Deploy.bat"
rm -f "$PUBLISH_PATH/DeployScript.ps1"
rm -f "$PUBLISH_PATH/deploy.sh"
rm -f "$PUBLISH_PATH/makefile"
rm -f "$PUBLISH_PATH/Dockerfile"
rm -f "$PUBLISH_PATH/.dockerignore"

# Remove development folders
rm -rf "$PUBLISH_PATH/.claude"
rm -rf "$PUBLISH_PATH/plans"
rm -rf "$PUBLISH_PATH/docs"
rm -rf "$PUBLISH_PATH/logs"
rm -rf "$PUBLISH_PATH/Tests"

# Remove Tests project artifacts
rm -f "$PUBLISH_PATH"/QLDA.*.Tests.*
rm -f "$PUBLISH_PATH"/*Tests*.dll
rm -f "$PUBLISH_PATH"/*Tests*.json
rm -f "$PUBLISH_PATH"/Moq.dll
rm -f "$PUBLISH_PATH"/xunit*.dll
rm -f "$PUBLISH_PATH"/coverlet*.dll
rm -f "$PUBLISH_PATH"/Microsoft.NET.Test*.dll

# Update web.config environment variable based on target
if [[ "$ENVIRONMENT" == "Staging" || "$ENVIRONMENT" == "staging" ]]; then
    sed -i 's/<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" \/>/<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Staging" \/>/g' "$PUBLISH_PATH/web.config"
fi
if [[ "$ENVIRONMENT" == "Production" || "$ENVIRONMENT" == "production" ]]; then
    sed -i 's/<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" \/>/<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" \/>/g' "$PUBLISH_PATH/web.config"
fi

echo "Cleanup completed."

# === Deploy Based on Environment ===
if [[ "$ENVIRONMENT" == "Production" || "$ENVIRONMENT" == "production" ]]; then
    echo ""
    echo "============================================"
    echo "  PRODUCTION BUILD COMPLETE"
    echo "============================================"
    echo "  Build artifacts available at:"
    echo "  $PUBLISH_PATH"
    echo ""
    echo "  NOTE: Production deployment requires manual"
    echo "  copy or CI/CD pipeline to deploy to server."
    echo "============================================"
else
    echo "Copying files to server..."

    COPY_SUCCESS=0

    # SMB server configuration
    # Credentials can be set via environment variables:
    #   SMB_SERVER (default: 192.168.1.12)
    #   SMB_USER (default: update)
    #   SMB_PASS (default: hardcoded fallback for internal network only)
    #   SMB_SHARE (default: API_MND)
    #   SMB_TARGET (default: TTCDS/QuanLyDuAn)
    SMB_SERVER="${SMB_SERVER:-192.168.1.12}"
    SMB_SHARE="${SMB_SHARE:-API_MND}"
    SMB_USER="${SMB_USER:-update}"
    SMB_PASS="${SMB_PASS:-Vietinfo@#@!}"
    SMB_TARGET="${SMB_TARGET:-TTCDS/QuanLyDuAn}"

    # Try using smbclient if available, otherwise fallback to rsync over smb mount
    if command -v smbclient &> /dev/null; then
        # Use smbclient for file copy
        echo "Using smbclient for file transfer..."
        echo "Server: $SMB_SERVER, Share: $SMB_SHARE, Target: $SMB_TARGET"
        # Use --directory flag to set target path, then upload all files with mput
        smbclient "//$SMB_SERVER/$SMB_SHARE" --directory="$SMB_TARGET" -U "$SMB_USER%$SMB_PASS" -c "prompt OFF; recurse ON; lcd \"$PUBLISH_PATH\"; mput *" 2>/dev/null
        COPY_SUCCESS=$?
    else
        # Fallback: try rsync if smb is mounted, or note the limitation
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
            echo "  sudo mount -t cifs //App01/API_MND/TTCDS/QuanLyDuAn /mnt/deploy -o username=YOUR_USER"
            COPY_SUCCESS=1
        fi
    fi

    if [[ $COPY_SUCCESS -eq 0 ]]; then
        echo ""
        echo "============================================"
        echo "  DEPLOYMENT COMPLETED SUCCESSFULLY"
        echo "============================================"
        echo "  Environment: $ENVIRONMENT"
        echo "  Mode: $DEPLOYMENT_MODE"
        echo "  Destination: $DESTINATION_PATH"
        echo "============================================"
    else
        echo "Deployment may have failed. Please check the output above."
    fi
fi
