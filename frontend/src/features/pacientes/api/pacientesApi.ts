import { httpClient } from '../../../core/api/httpClient'
import { toSearchParams } from '../../../shared/lib/queryString'
import type { PagedResult } from '../../../shared/types/api'
import type { PacienteDto } from '../types'

export interface PacienteInput {
  nombre: string
  apellido: string
  telefono: string
  obraSocial: string
}

export interface PacientesQuery {
  search?: string
  page?: number
  pageSize?: number
}

export const pacientesApi = {
  listar: (query: PacientesQuery) =>
    httpClient.get<PagedResult<PacienteDto>>(`/pacientes?${toSearchParams({ ...query })}`),
  obtener: (id: string) => httpClient.get<PacienteDto>(`/pacientes/${id}`),
  crear: (input: PacienteInput) => httpClient.post<PacienteDto>('/pacientes', input),
  editar: (id: string, input: PacienteInput) =>
    httpClient.put<PacienteDto>(`/pacientes/${id}`, input),
  eliminar: (id: string) => httpClient.delete<void>(`/pacientes/${id}`),
}
