#!/usr/bin/env bash

set -Eeuo pipefail

PACKAGE_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_SOURCE_DIR="${PACKAGE_ROOT}/app"
DB_SOURCE_DIR="${PACKAGE_ROOT}/db"
SERVICE_TEMPLATE_PATH="${PACKAGE_ROOT}/installer/rustman.service"

APP_ROOT="/opt/rustman"
APP_INSTALL_DIR="${APP_ROOT}/app"
DATA_ROOT="/var/lib/rustman"
STEAMCMD_DIR="${DATA_ROOT}/steamcmd"
RUST_RUNTIME_DIR="${DATA_ROOT}/rustdedicated"
INSTANCES_DIR="${DATA_ROOT}/instances"
MAP_PREVIEWS_DIR="${DATA_ROOT}/map-previews"
TMP_DIR="${DATA_ROOT}/tmp"
CONFIG_ROOT="/etc/rustman"
ENV_FILE_PATH="${CONFIG_ROOT}/rustman.env"
DATABASE_PATH="${DATA_ROOT}/rustman.db"
SERVICE_FILE_PATH="/etc/systemd/system/rustman.service"
STEAMCMD_DOWNLOAD_URL="https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz"
STEAMCMD_FALLBACK_URL="https://media.steampowered.com/installer/steamcmd_linux.tar.gz"
RUST_DEDICATED_APP_ID="258550"

CURRENT_STEP="startup"
ROLLBACK_ACTIVE=0
CREATED_GROUP=0
CREATED_USER=0
SERVICE_FILE_CREATED=0
SERVICE_ENABLED=0
SERVICE_STARTED=0
ENV_FILE_CREATED=0
STEAMCMD_INSTALLED=0
RUST_RUNTIME_INSTALLED=0
DATABASE_CREATED=0
APP_INSTALLED=0
CREATED_DIRECTORIES=()

log() {
    printf '[rustman-installer] %s\n' "$1"
}

fail() {
    printf '[rustman-installer] ERROR: %s\n' "$1" >&2
    exit 1
}

require_root() {
    if [[ "${EUID}" -ne 0 ]]; then
        fail "Installer must run as root."
    fi
}

ensure_source_layout() {
    [[ -d "${APP_SOURCE_DIR}" ]] || fail "Missing package directory: ${APP_SOURCE_DIR}"
    [[ -d "${DB_SOURCE_DIR}" ]] || fail "Missing package directory: ${DB_SOURCE_DIR}"
    [[ -f "${SERVICE_TEMPLATE_PATH}" ]] || fail "Missing service template: ${SERVICE_TEMPLATE_PATH}"
    [[ -f "${APP_SOURCE_DIR}/RustMan.Web" ]] || fail "Published Linux app host was not found in ${APP_SOURCE_DIR}"
}

assert_not_installed() {
    local existing=()

    getent group rustman >/dev/null && existing+=("group:rustman")
    getent passwd rustman >/dev/null && existing+=("user:rustman")
    [[ -e "${APP_ROOT}" ]] && existing+=("${APP_ROOT}")
    [[ -e "${DATA_ROOT}" ]] && existing+=("${DATA_ROOT}")
    [[ -e "${CONFIG_ROOT}" ]] && existing+=("${CONFIG_ROOT}")
    [[ -e "${SERVICE_FILE_PATH}" ]] && existing+=("${SERVICE_FILE_PATH}")

    if (( ${#existing[@]} > 0 )); then
        fail "RustMan already appears to be installed. Existing artifacts: ${existing[*]}"
    fi
}

install_os_prerequisites() {
    CURRENT_STEP="apt prerequisites"
    log "Installing OS prerequisites."
    export DEBIAN_FRONTEND=noninteractive
    apt-get update
    apt-get install -y ca-certificates curl tar sqlite3 lib32gcc-s1 lib32stdc++6
}

create_group_and_user() {
    CURRENT_STEP="system user"
    log "Creating rustman group."
    groupadd --system rustman
    CREATED_GROUP=1

    log "Creating rustman user."
    useradd \
        --system \
        --gid rustman \
        --home-dir "${DATA_ROOT}" \
        --no-create-home \
        --shell /usr/sbin/nologin \
        rustman
    CREATED_USER=1
}

track_directory_creation() {
    local path="$1"

    if [[ ! -e "${path}" ]]; then
        CREATED_DIRECTORIES+=("${path}")
    fi

    mkdir -p "${path}"
    chown rustman:rustman "${path}"
}

create_directories() {
    CURRENT_STEP="directory structure"
    log "Creating RustMan directories."

    track_directory_creation "${APP_ROOT}"
    track_directory_creation "${APP_INSTALL_DIR}"
    track_directory_creation "${DATA_ROOT}"
    track_directory_creation "${STEAMCMD_DIR}"
    track_directory_creation "${RUST_RUNTIME_DIR}"
    track_directory_creation "${INSTANCES_DIR}"
    track_directory_creation "${MAP_PREVIEWS_DIR}"
    track_directory_creation "${TMP_DIR}"
    track_directory_creation "${CONFIG_ROOT}"
}

deploy_application() {
    CURRENT_STEP="application deployment"
    log "Deploying RustMan application files."

    cp -a "${APP_SOURCE_DIR}/." "${APP_INSTALL_DIR}/"
    chown -R rustman:rustman "${APP_INSTALL_DIR}"
    chmod +x "${APP_INSTALL_DIR}/RustMan.Web"
    APP_INSTALLED=1
}

write_environment_file() {
    CURRENT_STEP="environment file"
    log "Writing RustMan environment file."

    cat > "${ENV_FILE_PATH}" <<EOF
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:5000
ConnectionStrings__Default=Data Source=${DATABASE_PATH}
RuntimeBootstrap__RconHost=127.0.0.1
EOF

    chown rustman:rustman "${ENV_FILE_PATH}"
    chmod 0640 "${ENV_FILE_PATH}"
    ENV_FILE_CREATED=1
}

apply_database_sql() {
    local sql_file="$1"

    log "Applying $(basename "${sql_file}")."
    sqlite3 "${DATABASE_PATH}" < "${sql_file}"
}

initialize_database() {
    CURRENT_STEP="database initialization"
    log "Creating SQLite database."

    : > "${DATABASE_PATH}"
    chown rustman:rustman "${DATABASE_PATH}"
    chmod 0640 "${DATABASE_PATH}"
    DATABASE_CREATED=1

    mapfile -t sql_files < <(find "${DB_SOURCE_DIR}" -maxdepth 1 -type f -name '*.sql' | sort)

    if (( ${#sql_files[@]} == 0 )); then
        fail "No SQL files were found in ${DB_SOURCE_DIR}."
    fi

    for sql_file in "${sql_files[@]}"; do
        apply_database_sql "${sql_file}"
    done
}

download_steamcmd_archive() {
    local destination="$1"
    local url="$2"

    curl --fail --location --silent --show-error --output "${destination}" "${url}"
}

install_steamcmd() {
    local archive_path="${TMP_DIR}/steamcmd_linux.tar.gz"

    CURRENT_STEP="steamcmd installation"
    log "Downloading SteamCMD."

    if ! download_steamcmd_archive "${archive_path}" "${STEAMCMD_DOWNLOAD_URL}"; then
        log "Primary SteamCMD URL failed, trying fallback URL."
        download_steamcmd_archive "${archive_path}" "${STEAMCMD_FALLBACK_URL}"
    fi

    log "Extracting SteamCMD into ${STEAMCMD_DIR}."
    tar -xzf "${archive_path}" -C "${STEAMCMD_DIR}"
    chown -R rustman:rustman "${STEAMCMD_DIR}"
    chmod +x "${STEAMCMD_DIR}/steamcmd.sh"
    rm -f "${archive_path}"
    STEAMCMD_INSTALLED=1
}

install_rust_runtime() {
    CURRENT_STEP="rust runtime installation"
    log "Downloading shared Rust dedicated server runtime."

    su -s /bin/bash rustman -c \
        "HOME='${STEAMCMD_DIR}' '${STEAMCMD_DIR}/steamcmd.sh' \
        +@ShutdownOnFailedCommand 1 \
        +@NoPromptForPassword 1 \
        +force_install_dir '${RUST_RUNTIME_DIR}' \
        +login anonymous \
        +app_update ${RUST_DEDICATED_APP_ID} validate \
        +quit"

    chown -R rustman:rustman "${RUST_RUNTIME_DIR}"
    RUST_RUNTIME_INSTALLED=1
}

install_service_file() {
    CURRENT_STEP="systemd unit"
    log "Installing RustMan systemd unit."

    install -m 0644 "${SERVICE_TEMPLATE_PATH}" "${SERVICE_FILE_PATH}"
    SERVICE_FILE_CREATED=1

    systemctl daemon-reload
}

enable_and_start_service() {
    CURRENT_STEP="systemd enable"
    log "Enabling RustMan service."
    systemctl enable rustman.service
    SERVICE_ENABLED=1

    CURRENT_STEP="systemd start"
    log "Starting RustMan service."
    systemctl start rustman.service
    SERVICE_STARTED=1
}

verify_success() {
    local instance_count
    local required_tables

    CURRENT_STEP="verification"
    log "Verifying installed artifacts."

    getent group rustman >/dev/null
    getent passwd rustman >/dev/null
    [[ -d "${APP_INSTALL_DIR}" ]]
    [[ -f "${ENV_FILE_PATH}" ]]
    [[ -f "${DATABASE_PATH}" ]]
    [[ -f "${STEAMCMD_DIR}/steamcmd.sh" ]]
    [[ -x "${RUST_RUNTIME_DIR}/RustDedicated" ]]
    [[ -f "${SERVICE_FILE_PATH}" ]]

    required_tables="$(sqlite3 "${DATABASE_PATH}" <<'SQL'
SELECT group_concat(name, ',')
FROM (
    SELECT name
    FROM sqlite_master
    WHERE type = 'table'
      AND name IN (
          'instances',
          'command_definitions',
          'command_parameters',
          'server_property_definitions',
          'server_property_allowed_values',
          'instance_property_values'
      )
    ORDER BY name
);
SQL
)"

    [[ "${required_tables}" == "command_definitions,command_parameters,instance_property_values,instances,server_property_allowed_values,server_property_definitions" ]]
    instance_count="$(sqlite3 "${DATABASE_PATH}" "SELECT COUNT(*) FROM instances;")"
    [[ "${instance_count}" == "0" ]]
    [[ "$(systemctl is-enabled rustman.service)" == "enabled" ]]
    [[ "$(systemctl is-active rustman.service)" == "active" ]]
}

rollback() {
    local path

    if (( ROLLBACK_ACTIVE == 1 )); then
        return
    fi

    ROLLBACK_ACTIVE=1
    log "Rollback started."

    if (( SERVICE_STARTED == 1 )); then
        systemctl stop rustman.service || true
    fi

    if (( SERVICE_ENABLED == 1 )); then
        systemctl disable rustman.service || true
    fi

    if (( SERVICE_FILE_CREATED == 1 )); then
        rm -f "${SERVICE_FILE_PATH}" || true
        systemctl daemon-reload || true
    fi

    if (( ENV_FILE_CREATED == 1 )); then
        rm -f "${ENV_FILE_PATH}" || true
    fi

    if (( RUST_RUNTIME_INSTALLED == 1 )); then
        rm -rf "${RUST_RUNTIME_DIR}" || true
    fi

    if (( STEAMCMD_INSTALLED == 1 )); then
        rm -rf "${STEAMCMD_DIR}" || true
    fi

    if (( DATABASE_CREATED == 1 )); then
        rm -f "${DATABASE_PATH}" || true
    fi

    if (( APP_INSTALLED == 1 )); then
        rm -rf "${APP_INSTALL_DIR}" || true
    fi

    for (( i=${#CREATED_DIRECTORIES[@]}-1; i>=0; i-- )); do
        path="${CREATED_DIRECTORIES[$i]}"
        rm -rf "${path}" || true
    done

    if (( CREATED_USER == 1 )); then
        userdel rustman || true
    fi

    if (( CREATED_GROUP == 1 )); then
        groupdel rustman || true
    fi

    log "Rollback finished."
}

handle_failure() {
    local exit_code="$1"
    log "Failure during ${CURRENT_STEP}."
    rollback
    exit "${exit_code}"
}

trap 'handle_failure $?' ERR

main() {
    require_root
    ensure_source_layout
    assert_not_installed
    install_os_prerequisites
    create_group_and_user
    create_directories
    deploy_application
    write_environment_file
    initialize_database
    install_steamcmd
    install_rust_runtime
    install_service_file
    enable_and_start_service
    verify_success
    log "Installation completed successfully."
}

main "$@"
