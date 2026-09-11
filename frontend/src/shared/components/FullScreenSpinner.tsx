import { Loader2 } from 'lucide-react'

/** Pantalla de carga durante la hidratación de sesión (sin chrome de layout ni flash de /login). */
export function FullScreenSpinner() {
  return (
    <div className="grid min-h-dvh place-items-center bg-canvas" role="status" aria-label="Cargando">
      <Loader2 size={28} className="animate-spin text-primary" aria-hidden="true" />
    </div>
  )
}
