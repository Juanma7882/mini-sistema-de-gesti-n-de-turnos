# Diagramas

Cuatro diagramas interactivos del sistema, generados con
[Archify](https://github.com/tt-a1i/archify) a partir del código de este repo.
Cada `.html` es **standalone**: se abre con doble clic, sin servidor ni
dependencias. Traen tema claro/oscuro, zoom, búsqueda, resaltado de relaciones
y exportación a PNG / SVG / WebM desde la barra superior.

| Archivo | Qué muestra |
| ------- | ----------- |
| [`arquitectura.html`](arquitectura.html) | Navegador → React SPA (Vercel) → Turnos.Api (Railway), las cuatro capas del backend, SQLite sobre el volume y el hub de SignalR. |
| [`estados-turno.html`](estados-turno.html) | La máquina de estados del turno: Pendiente → Confirmado → Atendido, la salida a Cancelado y el 409 de las transiciones ilegales. |
| [`auth-jwt-refresh.html`](auth-jwt-refresh.html) | Login, expiración del access token, refresh con rotación revoke-on-use y reintento del request original. |
| [`flujo-admin-profesional.html`](flujo-admin-profesional.html) | Qué hace cada rol desde el login, y dónde corta el servidor con 403 / 404 / 409. |

## Regenerar

Los `.json` al lado de cada HTML son la fuente. Para volver a generar uno:

```bash
npx skills add tt-a1i/archify -g https://github.com/tt-a1i/archify
cd ~/.agents/skills/archify
node bin/archify.mjs deliver architecture <ruta>/arquitectura.json <ruta>/arquitectura.html --quality showcase
```

Los tipos son `architecture`, `lifecycle`, `sequence` y `workflow`, en el mismo
orden que la tabla de arriba.

> La interfaz del visor (botones Light / Classic / Present / Export y las
> etiquetas de la leyenda) está en inglés: Archify solo localiza esos textos a
> inglés y chino. El contenido de los diagramas está íntegramente en español.
