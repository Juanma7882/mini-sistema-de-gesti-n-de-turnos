# Mejoras futuras

Cosas identificadas pero deliberadamente fuera del alcance de esta entrega
(deadline sábado 12/09/2026 22:30), con el motivo de dejarlas afuera.

## Seguridad y auth

- **Detección de reuso de refresh token.** Hoy la rotación es revoke-on-use
  (cada `/auth/refresh` revoca el token usado y emite uno nuevo), pero si un
  token robado se usa *antes* que el legítimo, no hay alarma — solo se nota
  que el legítimo ya está revocado. Una mejora real sería, al detectar el uso
  de un token ya revocado, revocar en cascada toda la cadena de ese usuario y
  forzar re-login.
- **Rate limiting / lockout en `/auth/login`.** No hay límite de intentos por
  IP o por cuenta; un ataque de fuerza bruta contra BCrypt es lento pero no
  está mitigado explícitamente.
- **Rol como enum cerrado (`Admin`/`Profesional`).** El modelo ya soporta un
  tercer rol sin cambios de schema (un `Usuario` puede no tener `Profesional`,
  como ya pasa con Admin), pero agregar `Supervisor`/`Dueño` real implica
  definir sus permisos — no se hizo porque no lo pedía el contrato.
## Datos y modelado

- **`Especialidad` (Profesional) y `ObraSocial` (Paciente) como texto libre.**
  No hay catálogo/tabla propia — dos admins pueden escribir "Cardiología" y
  "cardiologia" como valores distintos. Normalizar a una tabla con FK
  resolvería duplicados y typos, a costa de un CRUD extra.
- **Revertir un turno terminal.** `Cancelado`/`Atendido` son terminales para
  todos los roles sin excepción. Un caso real ("me equivoqué, lo cancelé mal")
  necesitaría una transición explícita con auditoría (quién, cuándo, por qué)
  en vez de simplemente reabrir el estado.
- **Paginación offset (`page`/`pageSize`), no cursor-based.** Para el volumen
  de datos de esta clínica de demo es irrelevante; con miles de turnos, un
  cursor evitaría el costo de `OFFSET` creciente en SQLite.

## Infraestructura y operación

- **SQLite + volume, no una base gestionada.** Válido para esta escala y
  mantiene el deploy simple (un solo contenedor, sin servicio de base
  aparte), pero no escala a múltiples instancias del backend (SQLite no
  soporta escritura concurrente desde varios procesos) ni tiene backups
  automáticos — para producción real, Postgres administrado.
- **Sin usuario no-root en el Dockerfile.** Se evaluó agregar `USER app`
  (la imagen `aspnet` ya trae uno desde .NET 8) pero se dejó afuera para no
  arriesgar un problema de permisos con el volume montado por Railway tan
  cerca del deadline — es un hardening de bajo esfuerzo, buen primer paso.
- **CI solo de backend.** `.github/workflows/backend-ci.yml` no incluye el
  frontend — Vercel ya corre su propio build check en cada PR/push como gate
  natural, así que no se justificó duplicar ese chequeo en GitHub Actions
  para esta entrega.
- **Health check más granular.** `GET /health` hoy es un "el proceso responde"
  liviano; no distingue *liveness* de *readiness* (por ejemplo, si la base no
  está migrada todavía). Para un entorno con checks automáticos de
  orquestador tendría sentido separarlos.
- **`ASPNETCORE_ENVIRONMENT=Development` en producción (Railway).** Decisión
  consciente para esta entrega (ver `docs/decisiones-tecnicas.md`) — en un
  despliegue real se revertiría a `Production` y, si se quisiera exponer
  documentación de la API, se optaría por Swagger detrás de auth o un
  archivo OpenAPI estático versionado en vez de la UI interactiva abierta.
