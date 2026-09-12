import { httpClient } from '../../../core/api/httpClient'
import { toSearchParams } from '../../../shared/lib/queryString'
import type { EstadoTurno, PagedResult } from '../../../shared/types/api'
import type { TurnoDto } from '../types'

export interface TurnosQuery {
  desde?: string
  hasta?: string
  estado?: EstadoTurno
  pacienteId?: string
  /** Solo lo usa Admin; para Profesional el servidor lo ignora. */
  profesionalId?: string
  page?: number
  pageSize?: number
}

export interface TurnoInput {
  pacienteId: number
  profesionalId: number
  inicio: string
  notas?: string
}

export const turnosApi = {
  listar: (query: TurnosQuery) =>
    httpClient.get<PagedResult<TurnoDto>>(`/turnos?${toSearchParams({ ...query })}`),
  obtener: (id: string) => httpClient.get<TurnoDto>(`/turnos/${id}`),
  crear: (input: TurnoInput) => httpClient.post<TurnoDto>('/turnos', input),
  editar: (id: string, input: TurnoInput) => httpClient.put<TurnoDto>(`/turnos/${id}`, input),
  cambiarEstado: (id: string, estado: EstadoTurno) =>
    httpClient.patch<TurnoDto>(`/turnos/${id}/estado`, { estado }),
}
