const formatter = new Intl.DateTimeFormat('es-AR', {
  dateStyle: 'medium',
  timeStyle: 'short',
})

/**
 * `inicio` llega como hora local naïve: "2026-09-15T15:00:00" (sin zona).
 * Se muestra tal cual; nunca se convierte a otra zona (arquitectura-frontend.md §6).
 */
export function formatInicio(inicio: string): string {
  const parsed = new Date(inicio)
  return Number.isNaN(parsed.getTime()) ? inicio : formatter.format(parsed)
}
