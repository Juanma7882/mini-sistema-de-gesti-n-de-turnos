#!/usr/bin/env bash
# Smoke test post-deploy (§11.5): login -> me -> crear paciente -> crear
# profesional -> crear turno -> cambiar estado, contra una Api ya desplegada.
#
# Uso:
#   BASE_URL=https://tu-app.up.railway.app \
#   ADMIN_PASSWORD=<Seed__AdminPassword real> \
#   ./scripts/smoke.sh
set -euo pipefail

BASE_URL="${BASE_URL:?Seteá BASE_URL, ej: BASE_URL=https://tu-app.up.railway.app}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@clinica.test}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:?Seteá ADMIN_PASSWORD con el valor real de Seed__AdminPassword en Railway}"

paso() { echo ""; echo "== $1 =="; }
fallo() { echo "FALLÓ: $1" >&2; exit 1; }
extraer_id() { sed -E 's/.*"id":([0-9]+).*/\1/'; }

paso "1) login"
login_body=$(curl -sf -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$ADMIN_EMAIL\",\"password\":\"$ADMIN_PASSWORD\"}") || fallo "POST /api/auth/login"
token=$(echo "$login_body" | sed -E 's/.*"token":"([^"]+)".*/\1/')
[ -n "$token" ] || fallo "no se pudo extraer el token del login"
echo "OK"

paso "2) me"
curl -sf "$BASE_URL/api/auth/me" -H "Authorization: Bearer $token" > /dev/null || fallo "GET /api/auth/me"
echo "OK"

sufijo=$(date +%s)

paso "3) crear paciente"
paciente_body=$(curl -sf -X POST "$BASE_URL/api/pacientes" \
  -H "Authorization: Bearer $token" -H "Content-Type: application/json" \
  -d "{\"nombre\":\"Smoke\",\"apellido\":\"Test-$sufijo\",\"telefono\":\"1100000000\",\"obraSocial\":\"OSDE\"}") \
  || fallo "POST /api/pacientes"
paciente_id=$(echo "$paciente_body" | extraer_id)
echo "OK, paciente id=$paciente_id"

paso "4) crear profesional"
profesional_body=$(curl -sf -X POST "$BASE_URL/api/profesionales" \
  -H "Authorization: Bearer $token" -H "Content-Type: application/json" \
  -d "{\"nombre\":\"Smoke\",\"apellido\":\"Profesional-$sufijo\",\"especialidad\":\"Clínica médica\"}") \
  || fallo "POST /api/profesionales"
profesional_id=$(echo "$profesional_body" | extraer_id)
echo "OK, profesional id=$profesional_id"

paso "5) crear turno"
inicio=$( (date -u -d "+7 days" +"%Y-%m-%dT10:00:00" 2>/dev/null) || (date -u -v+7d +"%Y-%m-%dT10:00:00") )
turno_body=$(curl -sf -X POST "$BASE_URL/api/turnos" \
  -H "Authorization: Bearer $token" -H "Content-Type: application/json" \
  -d "{\"pacienteId\":$paciente_id,\"profesionalId\":$profesional_id,\"inicio\":\"$inicio\"}") \
  || fallo "POST /api/turnos"
turno_id=$(echo "$turno_body" | extraer_id)
echo "OK, turno id=$turno_id"

paso "6) cambiar estado a Confirmado"
curl -sf -X PATCH "$BASE_URL/api/turnos/$turno_id/estado" \
  -H "Authorization: Bearer $token" -H "Content-Type: application/json" \
  -d '{"estado":"Confirmado"}' > /dev/null || fallo "PATCH /api/turnos/$turno_id/estado"
echo "OK"

echo ""
echo "Smoke test OK: login -> me -> paciente -> profesional -> turno -> cambiar estado"
