import { z } from 'zod'

export const profesionalSchema = z.object({
  nombre: z.string().min(1, 'Requerido.'),
  apellido: z.string().min(1, 'Requerido.'),
  especialidad: z.string().min(1, 'Requerido.'),
})

export type ProfesionalFormValues = z.infer<typeof profesionalSchema>
