import { httpClient } from '../../../core/api/httpClient'
import { toSearchParams } from '../../../shared/lib/queryString'
import type { PagedResult } from '../../../shared/types/api'
import type { ProfesionalDto } from '../types'

export interface ProfesionalInput {
  nombre: string
  apellido: string
  especialidad: string
}

/** Alta: crea el profesional junto con su usuario (Rol Profesional). */
export interface CrearProfesionalInput extends ProfesionalInput {
  email: string
  password: string
}

export interface ProfesionalesQuery {
  search?: string
  page?: number
  pageSize?: number
}

export const profesionalesApi = {
  listar: (query: ProfesionalesQuery) =>
    httpClient.get<PagedResult<ProfesionalDto>>(`/profesionales?${toSearchParams({ ...query })}`),
  obtener: (id: string) => httpClient.get<ProfesionalDto>(`/profesionales/${id}`),
  crear: (input: CrearProfesionalInput) => httpClient.post<ProfesionalDto>('/profesionales', input),
  editar: (id: string, input: ProfesionalInput) =>
    httpClient.put<ProfesionalDto>(`/profesionales/${id}`, input),
  eliminar: (id: string) => httpClient.delete<void>(`/profesionales/${id}`),
}
