import type { EstadoTurno } from '../../shared/types/api'

export interface PacienteResumen {
  id: string
  nombre: string
  apellido: string
  telefono: string
  obraSocial: string
}

export interface ProfesionalResumen {
  id: string
  nombre: string
  apellido: string
  especialidad: string
}

/** El TurnoDto embebe los resúmenes para renderizar el listado sin N+1 (README.md). */
export interface TurnoDto {
  id: string
  inicio: string
  estado: EstadoTurno
  notas: string | null
  paciente: PacienteResumen
  profesional: ProfesionalResumen
  createdAt: string
  updatedAt: string
}
