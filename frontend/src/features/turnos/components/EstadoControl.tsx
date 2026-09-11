import type { Role } from '../../../core/auth/session'
import type { EstadoTurno } from '../../../shared/types/api'
import { transicionesPermitidas } from '../machine'

interface EstadoControlProps {
  estado: EstadoTurno
  role: Role
  onCambiar: (siguiente: EstadoTurno) => void
}

/** Muestra solo las transiciones legales según estado + rol (arquitectura-frontend.md §3.6). */
export function EstadoControl({ estado, role, onCambiar }: EstadoControlProps) {
  const opciones = transicionesPermitidas(estado, role)

  if (opciones.length === 0) {
    return <span className="text-sm text-muted-foreground">{estado} · terminal</span>
  }

  return (
    <div className="flex gap-2">
      {opciones.map((siguiente) => (
        <button
          key={siguiente}
          onClick={() => onCambiar(siguiente)}
          className="rounded-md border px-2 py-1 text-xs"
        >
          → {siguiente}
        </button>
      ))}
    </div>
  )
}
