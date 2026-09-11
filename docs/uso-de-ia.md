# Uso de IA

## Herramienta

Claude Code (Sonnet 5), como agente dentro del editor — no autocompletado
puntual, sino la implementación completa del backend (capas §1-§9 del alcance
original) guiada por [`backend.md`](../backend.md) como lista de tareas viva:
cada sección se planificaba primero (se escribía el diseño y las decisiones
dentro de la propia tarea), se confirmaba, y recién ahí se implementaba,
verificaba con requests HTTP reales (no solo lectura de código) y se marcaba
hecha.

## Cómo se trabajó

Ritmo repetido en cada sección (§6 pipeline/auth, §7 endpoints de auth, §8
pacientes/profesionales, §9 turnos, §10 tests de integración, §11 deploy/CI,
§12 esta documentación):

1. **Plan** — antes de escribir código, quedaba por escrito en `backend.md`
   qué se iba a hacer y las decisiones de diseño no obvias (ej. controladores
   vs. minimal APIs, dónde va el prefijo `/api`, `Path` de la cookie de
   refresh).
2. **Implementación** — se escribía el código siguiendo el estilo ya
   establecido en el repo (convenciones de nombres, comentarios en español,
   XML doc en las clases públicas/no triviales).
3. **Verificación real, no solo lectura** — cada endpoint se probó con
   requests HTTP de verdad (`curl` contra la Api corriendo localmente:
   login → token → cada operación → status code y body exactos) antes de
   marcar la tarea como hecha. Varias veces esa verificación encontró bugs
   reales que una revisión de código sola no hubiera detectado (ver abajo).
4. **Tests automatizados** al final de cada bloque funcional (§10), que
   automatizan exactamente los mismos casos ya probados a mano.

## Prompts principales

Resumen representativo del flujo de la conversación (no transcripción
literal):

- *"revisa la siguiente tarea para el backend"* → primera revisión de
  `backend.md`, detectó huecos en el §6 planeado (faltaba quién dispara la
  validación de FluentValidation, el prefijo `/api`, decisión
  Controllers/Minimal APIs) antes de escribir una sola línea.
- *"planifica su implementación"* / *"si avanza"* / *"si"* → el patrón
  recurrente: primero planificar y dejarlo escrito, después una confirmación
  explícita para recién ahí implementar. Se repitió para §6 a §11.
- *"antes de continuar prepara los commits de este chat"* → history de commits
  Conventional Commits, separando deliberadamente el `fix(api)` del
  `ValidationFilter` de los `feat(api)` que lo rodean (reconstruyendo el
  archivo a su estado *pre-fix* para el primer commit).
- *"termina la parte 12 y luego lo hacemos"* → esta documentación.

## Qué se revisó y corrigió

Bugs reales encontrados durante la implementación (no solo casos de test
imaginados de antemano):

- **`page`/`pageSize` nunca llegaban desde el query string.** `[FromQuery]
  PageRequest page` volvía siempre con los defaults del record — el nombre
  del parámetro colisionaba con la propiedad `PageRequest.Page`. Se detectó
  pidiendo `?page=1&pageSize=2` y recibiendo el listado completo con
  `pageSize=20`. Corregido bindeando `int` sueltos y armando el `PageRequest`
  a mano.
- **Un enum inválido en el body tiraba 500, no 400.** Con el 400 automático
  de `[ApiController]` desactivado (`SuppressModelStateInvalidFilter`), un
  `{"estado":"NoExiste"}` dejaba `request` en `null` (fallo de *binding*, no
  de FluentValidation) y el controller explotaba con
  `NullReferenceException`. Confirmado en vivo con `curl` antes de escribir
  el fix; `ValidationFilter` ahora también revisa `ModelState`.
- **`MapInboundClaims` de JWT sin desactivar.** Hubiera roto
  `[Authorize(Roles=...)]` y la lectura de `sub` en `CurrentUser` en
  silencio — se detectó por revisión de la configuración de `AddJwtBearer`
  antes de que llegara a producir un bug en runtime, y se verificó con un
  login + `/auth/me` real que los claims llegaban íntegros.
- **Cookie de refresh con `Path` demasiado angosto.** El diseño original
  restringía `rt` a `/api/auth/refresh`; `logout` también necesita leerla
  para revocar y con ese `Path` el browser nunca la manda ahí. Corregido a
  `/api/auth`.
- **`WebApplicationFactory` no podía overridear `Jwt:Secret` en los tests de
  integración** por el orden en que `AddApi` lee `IConfiguration` (eager, al
  registrar servicios) vs. cuándo `ConfigureWebHost` aplica sus cambios
  (después). Resuelto seteando variables de entorno de proceso en el
  constructor del factory, que sí llegan a tiempo.
- **Falso positivo de bug**: un `POST /pacientes` con "gómez" tiraba 500 al
  probarlo con `curl -d` inline. Investigado antes de tocar código: era
  `curl`/git-bash en Windows mandando mal un carácter UTF-8 multibyte por
  argv, no un bug de la Api — confirmado mandando el mismo body desde un
  archivo (`--data-binary @archivo.json`), que funcionó perfecto.
- **Repositorio git corrupto** (rama `refs/heads/main` en ceros e índice con
  firma inválida, aparentemente un crash a mitad de un commit). Se recuperó
  vía reflog — encontró un commit "dangling" completo cuyo ref nunca se
  actualizó — y se restauró un archivo con corrupción de contenido real
  (una palabra suelta insertada en medio de una línea de código, ni
  compilaba) a su versión commiteada limpia. No es un bug de código, pero es
  el tipo de verificación de "¿el estado real coincide con lo que asumo?"
  que se aplicó en todo el proyecto.

## Supervisión humana

Cada decisión con más de una opción razonable (no una corrección objetiva de
bug) se presentó explícitamente para que la elija una persona, en vez de que
la IA la tomara sola: si mostrar Swagger en la URL pública de Railway
(`ASPNETCORE_ENVIRONMENT=Development` a propósito), el ritmo
planificar→confirmar→implementar en cada sección, y qué alcance dejar fuera.
La propuesta `unify-usuario-profesional` se evaluó primero como riesgosa tan
cerca del deadline y se pensó implementar solo su parte más chica; más
adelante, dentro del mismo plazo, se reconsideró y se implementó completa
(ver `openspec/changes/archive/2026-09-11-unify-usuario-profesional/`).
