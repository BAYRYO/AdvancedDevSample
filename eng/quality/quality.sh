#!/usr/bin/env bash
set -euo pipefail

GLOBAL_LINE_THRESHOLD="${GLOBAL_LINE_THRESHOLD:-55}"
INFRA_LINE_THRESHOLD="${INFRA_LINE_THRESHOLD:-30}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
SOLUTION="${REPO_ROOT}/AdvancedDevSample.slnx"
RESULTS_DIR="${REPO_ROOT}/TestResults"
RUN_RESULTS_DIR="${RESULTS_DIR}/quality-$(date -u +%Y%m%d%H%M%S)-$$"

cd "${REPO_ROOT}"

dotnet restore "${SOLUTION}"
dotnet build "${SOLUTION}" -nologo
dotnet test "${SOLUTION}" -nologo --collect:"XPlat Code Coverage" --settings "${REPO_ROOT}/eng/quality/coverage.runsettings" --results-directory "${RUN_RESULTS_DIR}"

COVERAGE_FILE="$(find "${RUN_RESULTS_DIR}" -name 'coverage.cobertura.xml' -printf '%T@ %p\n' | sort -n | tail -n 1 | cut -d' ' -f2-)"
if [[ -z "${COVERAGE_FILE}" ]]; then
  echo "Coverage file not found under ${RUN_RESULTS_DIR}." >&2
  exit 1
fi

GLOBAL_LINE_THRESHOLD="${GLOBAL_LINE_THRESHOLD}" INFRA_LINE_THRESHOLD="${INFRA_LINE_THRESHOLD}" "${REPO_ROOT}/eng/quality/check-coverage.sh" "${COVERAGE_FILE}"

dotnet format "${SOLUTION}" --verify-no-changes --severity error --verbosity minimal
