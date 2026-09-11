/** Estados del turno, en el mismo orden que el backend (README.md). */
export type EstadoTurno = 'Pendiente' | 'Confirmado' | 'Cancelado' | 'Atendido'

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

/** Cuerpo de error de la API (RFC 7807). */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}
