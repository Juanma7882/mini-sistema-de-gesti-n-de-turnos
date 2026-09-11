import { CheckCircle2, Clock, XCircle, ClipboardCheck, type LucideIcon } from 'lucide-react'
import type { EstadoTurno } from '../../../shared/types/api'

interface EstadoStyle {
  fill: string
  text: string
  icon: LucideIcon
  label: string
}

/** Mapa único estado -> estilo (frontend-visual-system spec, "Badges de estado del turno"). */
const ESTADOS: Record<EstadoTurno, EstadoStyle> = {
  Pendiente: { fill: '#FBE7D2', text: '#8A5A1C', icon: Clock, label: 'Pendiente' },
  Confirmado: { fill: '#F7E1EA', text: '#B03B6B', icon: CheckCircle2, label: 'Confirmado' },
  Atendido: { fill: '#DFEEE4', text: '#3E7A55', icon: ClipboardCheck, label: 'Atendido' },
  Cancelado: { fill: '#EEE9EB', text: '#6E6169', icon: XCircle, label: 'Cancelado' },
}

export function EstadoBadge({ estado }: { estado: EstadoTurno }) {
  const { fill, text, icon: Icon, label } = ESTADOS[estado]
  return (
    <span
      className="inline-flex w-fit items-center gap-1 rounded-md px-1.5 py-0.5 text-xs font-medium"
      style={{ backgroundColor: fill, color: text }}
    >
      <Icon size={12} aria-hidden="true" />
      {label}
    </span>
  )
}
