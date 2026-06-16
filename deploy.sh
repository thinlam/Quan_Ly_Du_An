#!/usr/bin/env bash
# ============================================
# Self-Deploy Script - QLDA Staging Deployment
# ============================================
#
# Quy trình:
#   1. Đọc credentials từ .env
#   2. Publish với Staging environment
#   3. Push lên SMB qua smbclient (không mount, không sudo)
#   4. Tạo/xử lý app_offline_.htm
#   5. Copy source sang destination
#   6. Đợi copy xong mới xóa app_offline
#
# Usage: ./deploy.sh
#
# Requirements:
#   - smbclient (apt install smbclient)
#   - .env file với SMB credentials
#
# ============================================

set -euo pipefail

# === Color Output ===
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# === Constants ===
MODULE="QLDA"
MODULE_NAME="QuanLyDuAn"
WEBAPI_PATH="./QLDA.WebApi/QLDA.WebApi.csproj"
ENVIRONMENT="Staging"

# === Load .env File ===
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="${SCRIPT_DIR}/.env"

if [[ -f "$ENV_FILE" ]]; then
    log_info "Loading credentials from .env..."
    set -a
    source "$ENV_FILE"
    set +a
else
    log_error ".env file not found at ${ENV_FILE}"
    exit 1
fi

# === Validate Required Env Variables ===
REQUIRED_VARS=("SMB_USERNAME" "SMB_PASSWORD" "SMB_DEST_QLDA")
for var in "${REQUIRED_VARS[@]}"; do
    if [[ -z "${!var:-}" ]]; then
        log_error "Missing required environment variable: $var"
        exit 1
    fi
done

# Parse SMB destination: \\\\192.168.1.12\\api_mnd\\TTCDS\\QuanLyDuAn
# Convert to: //192.168.1.12/api_mnd/TTCDS/QuanLyDuAn
DEST_UNC="$SMB_DEST_QLDA"
DEST_PATH="${DEST_UNC//\\\\/}"
DEST_HOST="${DEST_PATH%%\\*}"
DEST_SHARE_PATH="${DEST_PATH#*\\}"
DEST_SHARE="${DEST_SHARE_PATH%%\\*}"
DEST_REMOTE_PATH="${DEST_SHARE_PATH#*\\}"
SMB_URL="//${DEST_HOST}/${DEST_SHARE}"
DEST_REMOTE_PATH_SLASH="${DEST_REMOTE_PATH//\\/\/}"

log_info "SMB URL: $SMB_URL"
log_info "Remote path: $DEST_REMOTE_PATH"

# === Display Deployment Info ===
echo ""
echo "============================================"
echo -e "  ${GREEN}${MODULE}${NC} (${MODULE_NAME}) DEPLOYMENT"
echo "============================================"
echo -e "  ${BLUE}Environment${NC} : ${ENVIRONMENT}"
echo -e "  ${BLUE}Source${NC}      : ${WEBAPI_PATH}"
echo -e "  ${BLUE}Destination${NC} : ${SMB_URL}/${DEST_REMOTE_PATH}"
echo "============================================"
echo ""

# === Step 1: Build & Publish ===
log_info "Step 1/5: Building and publishing ${MODULE}.WebApi..."
PUBLISH_PATH="${SCRIPT_DIR}/bin/Release/net8.0/publish/${MODULE}"

mkdir -p "${SCRIPT_DIR}/bin/Release/net8.0/publish"

if ! dotnet publish "$WEBAPI_PATH" --configuration Release --output "$PUBLISH_PATH"; then
    log_error "Publish failed"
    exit 1
fi
log_success "Publish completed: ${PUBLISH_PATH}"

# === Step 2: Cleanup Publish Folder ===
log_info "Step 2/5: Cleaning up publish folder..."

# Remove dev config
[[ -f "$PUBLISH_PATH/appsettings.Development.json" ]] && rm -f "$PUBLISH_PATH/appsettings.Development.json"
rm -f "$PUBLISH_PATH/appsettings.Production.json"

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
log_info "Removing non-English localization folders..."
find "$PUBLISH_PATH" -maxdepth 1 -type d -regex '.*/[a-z]\{2\}\(-[A-Za-z]\+\)\?$' \
    ! -name 'en' ! -name 'en-US' ! -name 'en-GB' -exec rm -rf {} + 2>/dev/null || true

log_success "Cleanup completed"

# === Step 3: Update web.config ===
if [[ -f "$PUBLISH_PATH/web.config" ]]; then
    log_info "Step 3/5: Setting ASPNETCORE_ENVIRONMENT to ${ENVIRONMENT}..."
    sed -i "s|name=\"ASPNETCORE_ENVIRONMENT\" value=\"[^\"]*\"|name=\"ASPNETCORE_ENVIRONMENT\" value=\"${ENVIRONMENT}\"|g" \
        "${PUBLISH_PATH}/web.config"
    log_success "web.config updated"
fi

# === Step 4: Create app_offline_.htm if not exists ===
log_info "Step 4/5: Handling app_offline_.htm..."

APP_OFFLINE_SOURCE="${PUBLISH_PATH}/app_offline_.htm"

if [[ ! -f "$APP_OFFLINE_SOURCE" ]]; then
    log_info "Creating app_offline_.htm in source..."
    cat > "$APP_OFFLINE_SOURCE" << 'EOF'
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Application Offline</title>
</head>
<body style="font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #f5f5f5;">
    <h1 style="color: #e74c3c;">🔧 Đang bảo trì</h1>
    <p>Hệ thống đang được cập nhật. Vui lòng quay lại sau.</p>
    <p style="color: #666;">The application is under maintenance.</p>
</body>
</html>
EOF
    log_success "Created app_offline_.htm"
else
    log_info "app_offline_.htm already exists in source"
fi

# === Step 5: Deploy via smbclient (no mount, no sudo) ===
log_info "Step 5/5: Deploying to server..."

# Check for required tools
if ! command -v smbclient &> /dev/null; then
    log_error "smbclient not found. Install with: apt install smbclient"
    exit 1
fi

# smbclient auth line: -U user%pass
SMB_AUTH="-U=${SMB_USERNAME}%${SMB_PASSWORD}"

# Helper: run a single smbclient command non-interactively
# Usage: smb_run "put localfile remotefile" "del app_offline.htm"
smb_run() {
    smbclient "${SMB_URL}" "${SMB_AUTH}" \
        -c "cd ${DEST_REMOTE_PATH_SLASH}; $*" \
        </dev/null
}

# === Step 5a: Verify destination exists on SMB (mkdir if missing) ===
log_info "Verifying destination on SMB share..."
if ! smb_run "ls" >/dev/null 2>&1; then
    log_warn "Destination path does not exist, creating: ${SMB_URL}/${DEST_REMOTE_PATH}"
    if ! smb_run "mkdir \"${DEST_REMOTE_PATH_SLASH}\""; then
        log_error "Failed to create destination path on SMB share"
        log_error "Verify SMB_DEST_QLDA and credentials in .env"
        exit 1
    fi
fi
log_success "Destination OK: ${SMB_URL}/${DEST_REMOTE_PATH}"

# === Step 5b: Put site in maintenance mode ===
log_info "Uploading app_offline.htm (site goes down)..."

# Upload app_offline.htm — IIS uses this to take the app pool offline
if ! smb_run "put \"${APP_OFFLINE_SOURCE}\" \"app_offline.htm\""; then
    log_error "Failed to upload app_offline.htm"
    exit 1
fi
log_success "Site is now in maintenance mode"

# Wait for app pool to recycle
log_info "Waiting for app pool to recycle (3 seconds)..."
sleep 3

# === Step 5c: Upload source files (overwrite) ===
log_info "Uploading source files to destination..."

# Excluded dirs are already removed from local publish in Step 2, so a
# recursive mput of the publish folder is enough. `recurse on; prompt off`
# auto-answers yes and walks subdirectories; existing files are overwritten.
if ! smbclient "${SMB_URL}" "${SMB_AUTH}" \
        -c "cd ${DEST_REMOTE_PATH_SLASH}; lcd \"${PUBLISH_PATH}\"; recurse on; prompt off; mput *" \
        </dev/null; then
    log_error "Failed to upload source files via smbclient"
    exit 1
fi

log_success "Source files uploaded"

# Wait for file operations to complete
sleep 2

# === Step 5d: Remove app_offline.htm (restore site) ===
log_info "Removing app_offline.htm (site back online)..."

if ! smb_run "del \"app_offline.htm\""; then
    log_error "Failed to remove app_offline.htm"
    exit 1
fi

log_success "Site is restored and online"

# === Summary ===
echo ""
echo "============================================"
log_success "${MODULE} DEPLOYMENT SUCCESSFUL"
echo "============================================"
echo -e "  ${BLUE}Destination${NC}  : ${SMB_URL}/${DEST_REMOTE_PATH}"
echo "============================================"
echo ""
