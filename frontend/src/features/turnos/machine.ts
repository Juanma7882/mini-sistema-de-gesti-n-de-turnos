import type { Role } from '../../core/auth/session'
import type { EstadoTurno } from '../../shared/types/api'

/**
 * Transiciones legales de la máquina de estados del turno, espejo de
 * README.md → "Máquina de estados". `Cancelado` y `Atendido` son terminales.
 * El backend es la fuente de verdad; este mapa solo decide qué botones mostrar.
 */
const TRANSICIONES: Record<EstadoTurno, EstadoTurno[]> = {
  Pendiente: ['Confirmado', 'Cancelado'],
  Confirmado: ['Atendido', 'Cancelado'],
  Cancelado: [],
  Atendido: [],
}

/**
 * Ambos roles comparten el mismo set de transiciones legales; la diferencia
 * (Profesional solo sobre sus turnos) la aplica el servidor.
 */
export function transicionesPermitidas(desde: EstadoTurno, _role: Role): EstadoTurno[] {
  return TRANSICIONES[desde]
}
