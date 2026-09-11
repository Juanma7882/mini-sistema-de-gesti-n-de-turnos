import { Loader2 } from 'lucide-react'

/** Fallback de Suspense para rutas lazy dentro de MainLayout — a diferencia de FullScreenSpinner, no usa min-h-dvh para no tapar navbar/sidebar. */
export function RouteFallback() {
  return (
    <div className="grid min-h-[50vh] place-items-center" role="status" aria-label="Cargando">
      <Loader2 size={28} className="animate-spin text-primary" aria-hidden="true" />
    </div>
  )
}
