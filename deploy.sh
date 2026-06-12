#!/usr/bin/env bash
# ============================================
# Self-Deploy Script - QLDA Staging Deployment
# ============================================
#
# Quy trình:
#   1. Đọc credentials từ .env
#   2. Publish với Staging environment
#   3. Mount SMB share
#   4. Backup destination hiện tại (backup_YYYYMMDD_HHMMSS)
#   5. Tạo/xử lý app_offline_.htm
#   6. Copy source sang destination
#   7. Đợi copy xong mới xóa app_offline
#   8. Unmount SMB share
#
# Usage: ./deploy.sh
#
# Requirements:
#   - cifs-utils (apt install cifs-utils)
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
MOUNT_BASE="/tmp/smb_deploy_${MODULE}"

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

# Mount point for the module destination
DEST_REMOTE_PATH_SLASH="${DEST_REMOTE_PATH//\\/\/}"
MOUNT_POINT="${MOUNT_BASE}/${DEST_REMOTE_PATH_SLASH//\//_}"
DEST_PATH_LOCAL="${MOUNT_POINT}"

log_info "SMB URL: $SMB_URL"
log_info "Remote path: $DEST_REMOTE_PATH"
log_info "Mount point: $DEST_PATH_LOCAL"

# === Cleanup on Exit ===
cleanup_mount() {
    if mountpoint -q "$DEST_PATH_LOCAL" 2>/dev/null; then
        log_info "Unmounting SMB share..."
        $UMOUNT_CMD -l "$DEST_PATH_LOCAL" 2>/dev/null || true
    fi
    if [[ -d "$MOUNT_BASE" ]]; then
        rm -rf "$MOUNT_BASE" 2>/dev/null || true
    fi
}
trap cleanup_mount EXIT

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

# === Step 5: Deploy via Mount SMB ===
log_info "Step 5/5: Deploying to server..."

# Check for required tools
if ! command -v mount.cifs &> /dev/null; then
    log_error "mount.cifs not found. Install with: apt install cifs-utils"
    exit 1
fi

# Create backup folder name with timestamp
BACKUP_TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FOLDER="backup_${BACKUP_TIMESTAMP}"

# Create mount point
mkdir -p "$DEST_PATH_LOCAL"

# === Step 5a: Mount SMB share ===
log_info "Mounting SMB share..."

# Determine mount command (use sudo if not root)
if [[ $EUID -eq 0 ]]; then
    MOUNT_CMD="mount -t cifs"
    UMOUNT_CMD="umount"
else
    MOUNT_CMD="sudo mount -t cifs"
    UMOUNT_CMD="sudo umount"
fi

if ! $MOUNT_CMD "${SMB_URL}" "$DEST_PATH_LOCAL" -o username="${SMB_USERNAME}",password="${SMB_PASSWORD}",rw,uid=$(id -u),gid=$(id -g),file_mode=0777,dir_mode=0777,vers=3.0; then
    log_error "Failed to mount SMB share"
    log_error "Try: sudo mount -t cifs //192.168.1.12/api_mnd /tmp/test -o username=$SMB_USERNAME,password=***,vers=3.0"
    exit 1
fi
log_success "Mounted: ${SMB_URL} -> ${DEST_PATH_LOCAL}"

# Check if destination exists
if [[ ! -d "${DEST_PATH_LOCAL}/${DEST_REMOTE_PATH}" ]]; then
    log_error "Destination path does not exist: ${DEST_PATH_LOCAL}/${DEST_REMOTE_PATH}"
    umount "$DEST_PATH_LOCAL"
    exit 1
fi

DEST_ACTUAL="${DEST_PATH_LOCAL}/${DEST_REMOTE_PATH}"

# === Step 5b: Backup current destination ===
log_info "Creating backup: $BACKUP_FOLDER"

# Create backup folder
mkdir -p "${DEST_ACTUAL}/${BACKUP_FOLDER}"

# Copy all files/folders to backup (exclude backup_* and logs)
shopt -s dotglob
shopt -s nullglob
for item in "${DEST_ACTUAL}"/*; do
    name=$(basename "$item")
    # Skip backup folders, logs, app_offline
    [[ "$name" == backup_* ]] && continue
    [[ "$name" == "logs" ]] && continue
    [[ "$name" == app_offline* ]] && continue
    cp -a "$item" "${DEST_ACTUAL}/${BACKUP_FOLDER}/"
done
shopt -u dotglob

BACKUP_COUNT=$(find "${DEST_ACTUAL}/${BACKUP_FOLDER}" -type f 2>/dev/null | wc -l)
log_success "Backup completed: ${BACKUP_COUNT} files copied to ${BACKUP_FOLDER}"

# === Step 5c: Handle app_offline.htm ===
log_info "Putting site in maintenance mode (app_offline.htm)..."

# Copy app_offline_.htm as app_offline.htm (site goes down)
cp "$APP_OFFLINE_SOURCE" "${DEST_ACTUAL}/app_offline.htm"

log_success "Site is now in maintenance mode"

# Wait for app pool to recycle
log_info "Waiting for app pool to recycle (3 seconds)..."
sleep 3

# === Step 5d: Copy source to destination ===
log_info "Copying source files to destination..."

# Use rsync for efficient and reliable copy
if command -v rsync &> /dev/null; then
    rsync -av --progress --exclude '.claude' --exclude 'plans' --exclude 'docs' --exclude 'logs' --exclude 'Tests' --exclude 'backup' "${PUBLISH_PATH}/" "${DEST_ACTUAL}/"
else
    # Fallback to cp
    cp -a "${PUBLISH_PATH}/." "${DEST_ACTUAL}/"
fi

log_success "Source files copied"

# Wait for file operations to complete
sync
sleep 2

# === Step 5e: Remove app_offline.htm (restore site) ===
log_info "Removing app_offline.htm (restore mode)..."

rm -f "${DEST_ACTUAL}/app_offline.htm"

# Copy app_offline_.htm back for next deployment
cp "$APP_OFFLINE_SOURCE" "${DEST_ACTUAL}/app_offline_.htm"

log_success "Site is restored and online"

# === Summary ===
echo ""
echo "============================================"
log_success "${MODULE} DEPLOYMENT SUCCESSFUL"
echo "============================================"
echo -e "  ${BLUE}Destination${NC}  : ${DEST_ACTUAL}"
echo -e "  ${BLUE}Backup${NC}       : ${DEST_ACTUAL}/${BACKUP_FOLDER}"
echo -e "  ${BLUE}Backup files${NC} : ${BACKUP_COUNT:-0} files"
echo "============================================"
echo ""
