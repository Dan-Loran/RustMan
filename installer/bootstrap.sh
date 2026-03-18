#!/usr/bin/env bash

set -Eeuo pipefail

DEFAULT_INSTALLER_URL="https://github.com/Dan-Loran/RustMan/releases/latest/download/rustman-installer.tar.gz"
INSTALLER_URL="${RUSTMAN_INSTALLER_URL:-${DEFAULT_INSTALLER_URL}}"
WORK_DIR="$(mktemp -d)"
ARCHIVE_PATH="${WORK_DIR}/rustman-installer.tar.gz"

cleanup() {
    rm -rf "${WORK_DIR}"
}

trap cleanup EXIT

if [[ "${EUID}" -ne 0 ]]; then
    if ! command -v sudo >/dev/null 2>&1; then
        printf '[rustman-bootstrap] ERROR: sudo is required when bootstrap is not run as root.\n' >&2
        exit 1
    fi

    SUDO="sudo"
else
    SUDO=""
fi

printf '[rustman-bootstrap] Downloading %s\n' "${INSTALLER_URL}"
if ! curl --fail --location --silent --show-error --output "${ARCHIVE_PATH}" "${INSTALLER_URL}"; then
    printf '[rustman-bootstrap] ERROR: Failed to download installer asset from %s\n' "${INSTALLER_URL}" >&2
    printf '[rustman-bootstrap] ERROR: Check that the GitHub release asset exists at https://github.com/Dan-Loran/RustMan/releases\n' >&2
    exit 1
fi

printf '[rustman-bootstrap] Extracting installer package.\n'
tar -xzf "${ARCHIVE_PATH}" -C "${WORK_DIR}"

printf '[rustman-bootstrap] Running installer.\n'
${SUDO} bash "${WORK_DIR}/installer/install.sh"
