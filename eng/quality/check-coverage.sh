#!/usr/bin/env bash
set -euo pipefail

COVERAGE_FILE="${1:-}"
GLOBAL_THRESHOLD="${GLOBAL_LINE_THRESHOLD:-55}"
INFRA_THRESHOLD="${INFRA_LINE_THRESHOLD:-30}"

if [[ -z "${COVERAGE_FILE}" ]]; then
  COVERAGE_FILE="$(find . -name 'coverage.cobertura.xml' -printf '%T@ %p\n' | sort -n | tail -n 1 | cut -d' ' -f2-)"
fi

if [[ -z "${COVERAGE_FILE}" || ! -f "${COVERAGE_FILE}" ]]; then
  echo "Coverage file not found. Run tests with --collect:\"XPlat Code Coverage\" first." >&2
  exit 1
fi

global_rate_raw="$(sed -n 's/.*<coverage[^>]*line-rate="\([0-9.]*\)".*/\1/p' "${COVERAGE_FILE}" | head -n 1)"
infra_rate_raw="$(sed -n 's/.*<package name="AdvancedDevSample.Infrastructure"[^>]*line-rate="\([0-9.]*\)".*/\1/p' "${COVERAGE_FILE}" | head -n 1)"

if [[ -z "${global_rate_raw}" || -z "${infra_rate_raw}" ]]; then
  echo "Unable to parse coverage rates from ${COVERAGE_FILE}." >&2
  exit 1
fi

global_pct="$(awk -v r="${global_rate_raw}" 'BEGIN { printf "%.2f", r * 100 }')"
infra_pct="$(awk -v r="${infra_rate_raw}" 'BEGIN { printf "%.2f", r * 100 }')"

echo "Coverage file: ${COVERAGE_FILE}"
echo
echo "Coverage thresholds:"
echo "  Global line rate:         ${global_pct}% (required >= ${GLOBAL_THRESHOLD}%)"
echo "  Infrastructure line rate: ${infra_pct}% (required >= ${INFRA_THRESHOLD}%)"
echo

global_ok="$(awk -v a="${global_pct}" -v b="${GLOBAL_THRESHOLD}" 'BEGIN { print (a >= b) ? 1 : 0 }')"
infra_ok="$(awk -v a="${infra_pct}" -v b="${INFRA_THRESHOLD}" 'BEGIN { print (a >= b) ? 1 : 0 }')"

if [[ "${global_ok}" -ne 1 ]]; then
  echo "FAIL: global line rate is below threshold." >&2
  exit 1
fi

if [[ "${infra_ok}" -ne 1 ]]; then
  echo "FAIL: infrastructure line rate is below threshold." >&2
  exit 1
fi

echo "PASS: coverage thresholds satisfied."
