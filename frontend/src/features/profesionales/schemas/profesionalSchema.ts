import { z } from 'zod'

export const profesionalSchema = z.object({
  nombre: z.string().min(1, 'Requerido.'),
  apellido: z.string().min(1, 'Requerido.'),
  especialidad: z.string().min(1, 'Requerido.'),
})

/** Alta: crea el profesional junto con su usuario, por eso pide también las
 * credenciales de acceso. */
export const crearProfesionalSchema = profesionalSchema.extend({
  email: z.string().min(1, 'Requerido.').email('Email inválido.'),
  password: z.string().min(8, 'Mínimo 8 caracteres.'),
})

export type ProfesionalFormValues = z.infer<typeof crearProfesionalSchema>
