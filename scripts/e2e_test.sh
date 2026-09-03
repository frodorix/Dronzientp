#!/usr/bin/env bash

# ==============================================================================
# AeroRoute End-to-End (E2E) Test Suite using Bash & Curl
# ==============================================================================

set -euo pipefail

# Color Codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m' # No Color

PORT="${PORT:-5197}"
BASE_URL="http://localhost:${PORT}"
SPAWNED_PID=""

log_info() {
    echo -e "${CYAN}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

cleanup() {
    if [ -n "${SPAWNED_PID}" ]; then
        log_info "Stopping spawned background server (PID: ${SPAWNED_PID})..."
        kill "${SPAWNED_PID}" 2>/dev/null || true
        wait "${SPAWNED_PID}" 2>/dev/null || true
    fi
}
trap cleanup EXIT

# ------------------------------------------------------------------------------
# 1. Ensure Application Server is Running
# ------------------------------------------------------------------------------
log_info "Checking if AeroRoute Web App is running at ${BASE_URL}..."

if ! curl -s --connect-timeout 2 "${BASE_URL}/" > /dev/null; then
    log_warn "App not currently responding at ${BASE_URL}. Launching local web app..."
    
    # Start web app in background
    dotnet run --project src/DroneDeliveryApp.Web --urls "${BASE_URL}" > /dev/null 2>&1 &
    SPAWNED_PID=$!
    log_info "App started in background with PID ${SPAWNED_PID}. Waiting for server readiness..."

    MAX_RETRIES=20
    RETRY_COUNT=0
    while ! curl -s --connect-timeout 1 "${BASE_URL}/" > /dev/null; do
        sleep 0.5
        RETRY_COUNT=$((RETRY_COUNT + 1))
        if [ "$RETRY_COUNT" -ge "$MAX_RETRIES" ]; then
            log_error "Server failed to start after ${MAX_RETRIES} attempts."
            exit 1
        fi
    done
fi

log_success "AeroRoute server is UP and responding at ${BASE_URL}!"

# Create temp workspace for test artifacts
TMP_DIR=$(mktemp -d)
trap 'rm -rf "${TMP_DIR}"; cleanup' EXIT

# ------------------------------------------------------------------------------
# 2. Test GET /api/delivery/sample-csv (Sample CSV Generation)
# ------------------------------------------------------------------------------
log_info "Testing GET /api/delivery/sample-csv (Standard format)..."
SAMPLE_FILE="${TMP_DIR}/sample_standard.csv"
HTTP_STATUS=$(curl -s -w "%{http_code}" -o "${SAMPLE_FILE}" "${BASE_URL}/api/delivery/sample-csv?format=standard")

if [ "${HTTP_STATUS}" -eq 200 ] && [ -s "${SAMPLE_FILE}" ]; then
    log_success "Sample CSV downloaded successfully (${HTTP_STATUS})."
else
    log_error "Failed to download sample CSV. Status: ${HTTP_STATUS}"
    exit 1
fi

# ------------------------------------------------------------------------------
# 3. Test POST /api/delivery/plan (Standard CSV Upload & Plan Generation)
# ------------------------------------------------------------------------------
log_info "Testing POST /api/delivery/plan with sample file upload..."
RESPONSE_FILE="${TMP_DIR}/plan_response.json"

HTTP_STATUS=$(curl -s -w "%{http_code}" -o "${RESPONSE_FILE}" \
    -F "file=@${SAMPLE_FILE}" \
    "${BASE_URL}/api/delivery/plan")

if [ "${HTTP_STATUS}" -ne 200 ]; then
    log_error "POST /api/delivery/plan failed with status ${HTTP_STATUS}."
    cat "${RESPONSE_FILE}"
    exit 1
fi

# Extract fields using python / grep
PLAN_ID=$(grep -o '"id":"[^"]*' "${RESPONSE_FILE}" | cut -d'"' -f4 || echo "")
TOTAL_DRONES=$(grep -o '"totalDrones":[0-9]*' "${RESPONSE_FILE}" | cut -d':' -f2 || echo "0")
TOTAL_TRIPS=$(grep -o '"totalTrips":[0-9]*' "${RESPONSE_FILE}" | cut -d':' -f2 || echo "0")

if [ -n "${PLAN_ID}" ] && [ "${TOTAL_DRONES}" -gt 0 ]; then
    log_success "Delivery Plan generated! Plan ID: ${PLAN_ID} | Drones: ${TOTAL_DRONES} | Trips: ${TOTAL_TRIPS}"
else
    log_error "Plan response missing ID or valid drone plans."
    cat "${RESPONSE_FILE}"
    exit 1
fi

# ------------------------------------------------------------------------------
# 4. Test POST /api/delivery/plan (Tabular CSV Format Upload)
# ------------------------------------------------------------------------------
log_info "Testing POST /api/delivery/plan with Tabular CSV Format..."
TABULAR_CSV="${TMP_DIR}/sample_tabular.csv"
cat << 'EOF' > "${TABULAR_CSV}"
Type,Name,Weight,Location
Drone,HeavyDrone,500,
Drone,LightDrone,150,
Package,Pkg-1,200,Location X
Package,Pkg-2,100,Location X
Package,Pkg-3,80,Location Y
EOF

TABULAR_RESPONSE="${TMP_DIR}/tabular_response.json"
HTTP_STATUS=$(curl -s -w "%{http_code}" -o "${TABULAR_RESPONSE}" \
    -F "file=@${TABULAR_CSV}" \
    "${BASE_URL}/api/delivery/plan")

if [ "${HTTP_STATUS}" -eq 200 ]; then
    TABULAR_PLAN_ID=$(grep -o '"id":"[^"]*' "${TABULAR_RESPONSE}" | cut -d'"' -f4 || echo "")
    log_success "Tabular CSV parsed and planned successfully! Plan ID: ${TABULAR_PLAN_ID}"
else
    log_error "Failed to process tabular CSV. Status: ${HTTP_STATUS}"
    cat "${TABULAR_RESPONSE}"
    exit 1
fi

# ------------------------------------------------------------------------------
# 5. Test GET /api/delivery/plans (Retrieve History)
# ------------------------------------------------------------------------------
log_info "Testing GET /api/delivery/plans (Recent History)..."
HISTORY_FILE="${TMP_DIR}/history.json"
HTTP_STATUS=$(curl -s -w "%{http_code}" -o "${HISTORY_FILE}" "${BASE_URL}/api/delivery/plans")

if [ "${HTTP_STATUS}" -eq 200 ] && grep -q "${PLAN_ID}" "${HISTORY_FILE}"; then
    log_success "Recent plans list retrieved. Plan ID ${PLAN_ID} found in repository."
else
    log_error "Plan ID ${PLAN_ID} not found in history list."
    cat "${HISTORY_FILE}"
    exit 1
fi

# ------------------------------------------------------------------------------
# 6. Test GET /api/delivery/plans/{id} (Get Plan Details)
# ------------------------------------------------------------------------------
log_info "Testing GET /api/delivery/plans/${PLAN_ID}..."
DETAIL_FILE="${TMP_DIR}/detail.json"
HTTP_STATUS=$(curl -s -w "%{http_code}" -o "${DETAIL_FILE}" "${BASE_URL}/api/delivery/plans/${PLAN_ID}")

if [ "${HTTP_STATUS}" -eq 200 ] && grep -q "${PLAN_ID}" "${DETAIL_FILE}"; then
    log_success "Plan detail retrieved successfully for ID ${PLAN_ID}."
else
    log_error "Failed to retrieve plan detail for ID ${PLAN_ID}."
    cat "${DETAIL_FILE}"
    exit 1
fi

# ------------------------------------------------------------------------------
# 7. Test Edge Case: Overweight Package Detection
# ------------------------------------------------------------------------------
log_info "Testing Edge Case: Package exceeding max drone capacity..."
OVERWEIGHT_CSV="${TMP_DIR}/overweight.csv"
cat << 'EOF' > "${OVERWEIGHT_CSV}"
DroneSmall, 100
LocationA, 50
LocationOverweight, 999
EOF

OVERWEIGHT_RESPONSE="${TMP_DIR}/overweight_response.json"
HTTP_STATUS=$(curl -s -w "%{http_code}" -o "${OVERWEIGHT_RESPONSE}" \
    -F "file=@${OVERWEIGHT_CSV}" \
    "${BASE_URL}/api/delivery/plan")

if [ "${HTTP_STATUS}" -eq 200 ] && grep -q "unassignedPackages" "${OVERWEIGHT_RESPONSE}" && grep -q "LocationOverweight" "${OVERWEIGHT_RESPONSE}"; then
    log_success "Overweight package properly detected and added to unassigned list!"
else
    log_error "Overweight package test failed."
    cat "${OVERWEIGHT_RESPONSE}"
    exit 1
fi

# ------------------------------------------------------------------------------
# 8. Test DELETE /api/delivery/plans/{id} (Delete Plan)
# ------------------------------------------------------------------------------
log_info "Testing DELETE /api/delivery/plans/${PLAN_ID}..."
DELETE_RESPONSE="${TMP_DIR}/delete_response.json"
HTTP_STATUS=$(curl -s -w "%{http_code}" -o "${DELETE_RESPONSE}" \
    -X DELETE "${BASE_URL}/api/delivery/plans/${PLAN_ID}")

if [ "${HTTP_STATUS}" -eq 200 ]; then
    log_success "Plan ${PLAN_ID} deleted successfully."
else
    log_error "Failed to delete plan ${PLAN_ID}. Status: ${HTTP_STATUS}"
    cat "${DELETE_RESPONSE}"
    exit 1
fi

# Verify deletion
NOT_FOUND_STATUS=$(curl -s -w "%{http_code}" -o /dev/null "${BASE_URL}/api/delivery/plans/${PLAN_ID}")
if [ "${NOT_FOUND_STATUS}" -eq 404 ]; then
    log_success "Verified deleted plan returns 404 Not Found as expected."
else
    log_error "Expected 404 for deleted plan, got ${NOT_FOUND_STATUS}."
    exit 1
fi

# ------------------------------------------------------------------------------
# Final Summary
# ------------------------------------------------------------------------------
echo ""
echo -e "${BOLD}${GREEN}========================================================================${NC}"
echo -e "${BOLD}${GREEN}  ALL E2E INTEGRATION TESTS PASSED SUCCESSFULLY!  ${NC}"
echo -e "${BOLD}${GREEN}========================================================================${NC}"
echo ""
