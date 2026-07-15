#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT="$ROOT/publish"
PROD_SETTINGS="$ROOT/src/Tacdent.Api/appsettings.Production.json"

if [[ ! -f "$PROD_SETTINGS" ]]; then
  echo "Missing $PROD_SETTINGS"
  echo "Copy appsettings.Production.example.json and fill in hosting values first."
  exit 1
fi

if grep -q '<host-verdigi-sunucu>' "$PROD_SETTINGS"; then
  echo "appsettings.Production.json still has placeholder values."
  echo "Fill in connection string, secrets, and Cors:Origins before publishing."
  exit 1
fi

echo "Publishing Tacdent.Api (Release, win-x64, self-contained)..."
dotnet publish "$ROOT/src/Tacdent.Api" \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -o "$OUTPUT"

WEB_CONFIG="$OUTPUT/web.config"
if [[ ! -f "$WEB_CONFIG" ]]; then
  echo "Expected web.config in publish output but it was not generated."
  exit 1
fi

# Ensure Production environment and stdout logging for IIS troubleshooting.
python3 - "$WEB_CONFIG" <<'PY'
import sys
import xml.etree.ElementTree as ET

path = sys.argv[1]
tree = ET.parse(path)
root = tree.getroot()

asp_net_core = root.find(".//aspNetCore")
if asp_net_core is None:
    raise SystemExit("aspNetCore element not found in web.config")

asp_net_core.set("stdoutLogEnabled", "true")
asp_net_core.set("stdoutLogFile", ".\\logs\\stdout")
asp_net_core.set("hostingModel", "outofprocess")

env_vars = asp_net_core.find("environmentVariables")
if env_vars is None:
    env_vars = ET.SubElement(asp_net_core, "environmentVariables")

existing = {
    item.get("name"): item
    for item in env_vars.findall("environmentVariable")
}

def set_env(name: str, value: str) -> None:
    if name in existing:
        existing[name].set("value", value)
    else:
        ET.SubElement(env_vars, "environmentVariable", {"name": name, "value": value})

set_env("ASPNETCORE_ENVIRONMENT", "Production")

tree.write(path, encoding="utf-8", xml_declaration=True)
PY

mkdir -p "$OUTPUT/logs"
cp "$PROD_SETTINGS" "$OUTPUT/appsettings.Production.json"

echo ""
echo "Publish complete: $OUTPUT"
echo "Upload the entire publish/ folder to your IIS site root via FileZilla."
echo "See DEPLOY.md for FTP, SSL, and verification steps."
