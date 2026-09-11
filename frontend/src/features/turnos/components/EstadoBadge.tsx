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
  // #3B7451 en vez de #3E7A55: mismo verde salvia, oscurecido lo mínimo para
  // llegar a 4.5:1 AA sobre el fill (antes 4.24:1; verificado con WCAG, no a ojo).
  Atendido: { fill: '#DFEEE4', text: '#3B7451', icon: ClipboardCheck, label: 'Atendido' },
  Cancelado: { fill: '#EEE9EB', text: '#6E6169', icon: XCircle, label: 'Cancelado' },
}

export function EstadoBadge({ estado }: { estado: EstadoTurno }) {
  const { fill, text, icon: Icon, label } = ESTADOS[estado]
  return (
    <span
      className="inline-flex w-fit items-center gap-1 rounded-md px-1.5 py-0.5 text-meta font-medium"
      style={{ backgroundColor: fill, color: text }}
    >
      <Icon size={12} aria-hidden="true" />
      {label}
    </span>
  )
}
