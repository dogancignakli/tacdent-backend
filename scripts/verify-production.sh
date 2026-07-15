#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 https://api.example.com"
  echo "Verifies production API after FTP deploy."
  exit 1
fi

BASE_URL="${1%/}"
SERVICES_URL="$BASE_URL/api/services"
LOGIN_URL="$BASE_URL/api/auth/login"

echo "GET $SERVICES_URL"
HTTP_CODE="$(curl -sS -o /tmp/tacdent-services.json -w "%{http_code}" "$SERVICES_URL")"
echo "Status: $HTTP_CODE"
if [[ "$HTTP_CODE" != "200" ]]; then
  echo "Expected 200 from /api/services"
  cat /tmp/tacdent-services.json || true
  exit 1
fi

head -c 200 /tmp/tacdent-services.json
echo ""
echo ""

if [[ -n "${TACDENT_ADMIN_EMAIL:-}" && -n "${TACDENT_ADMIN_PASSWORD:-}" ]]; then
  echo "POST $LOGIN_URL"
  LOGIN_CODE="$(curl -sS -o /tmp/tacdent-login.json -w "%{http_code}" \
    -X POST "$LOGIN_URL" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$TACDENT_ADMIN_EMAIL\",\"password\":\"$TACDENT_ADMIN_PASSWORD\",\"recaptchaToken\":\"\"}")"
  echo "Status: $LOGIN_CODE"
  if [[ "$LOGIN_CODE" != "200" ]]; then
    echo "Login check failed (reCAPTCHA may be enabled — disable temporarily or pass a token)."
    cat /tmp/tacdent-login.json || true
    exit 1
  fi
  head -c 200 /tmp/tacdent-login.json
  echo ""
else
  echo "Skip login check: set TACDENT_ADMIN_EMAIL and TACDENT_ADMIN_PASSWORD to test POST /api/auth/login."
fi

SCALAR_CODE="$(curl -sS -o /dev/null -w "%{http_code}" "$BASE_URL/scalar")"
echo "GET $BASE_URL/scalar -> $SCALAR_CODE (404 expected in Production)"

echo "Production checks passed."
