import { Check, ClipboardCheck, XCircle, type LucideIcon } from 'lucide-react'
import type { Role } from '../../../core/auth/session'
import type { EstadoTurno } from '../../../shared/types/api'
import { transicionesPermitidas } from '../machine'

interface EstadoControlProps {
  estado: EstadoTurno
  role: Role
  onCambiar: (siguiente: EstadoTurno) => void
}

const ACCION: Record<EstadoTurno, { label: string; icon: LucideIcon; destructive?: boolean }> = {
  Confirmado: { label: 'Confirmar', icon: Check },
  Atendido: { label: 'Marcar atendido', icon: ClipboardCheck },
  Cancelado: { label: 'Cancelar', icon: XCircle, destructive: true },
  Pendiente: { label: 'Pendiente', icon: Check },
}

/** Muestra solo las transiciones legales según estado + rol (frontend-turnos-views spec). */
export function EstadoControl({ estado, role, onCambiar }: EstadoControlProps) {
  const opciones = transicionesPermitidas(estado, role)

  if (opciones.length === 0) {
    return <span className="text-sm text-muted-foreground">Sin acciones disponibles</span>
  }

  return (
    <div className="flex flex-wrap gap-2">
      {opciones.map((siguiente) => {
        const { label, icon: Icon, destructive } = ACCION[siguiente]
        return (
          <button
            key={siguiente}
            type="button"
            onClick={() => onCambiar(siguiente)}
            className={`flex items-center gap-1.5 rounded-lg border px-2.5 py-1.5 text-meta font-medium ${
              destructive
                ? 'border-destructive text-destructive hover:bg-primary-tint'
                : 'border-border text-foreground hover:bg-primary-tint'
            }`}
          >
            <Icon size={14} aria-hidden="true" />
            {label}
          </button>
        )
      })}
    </div>
  )
}
