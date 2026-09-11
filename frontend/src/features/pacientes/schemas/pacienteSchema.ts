import { z } from 'zod'

export const pacienteSchema = z.object({
  nombre: z.string().min(1, 'Requerido.'),
  apellido: z.string().min(1, 'Requerido.'),
  telefono: z.string().min(6, 'Teléfono inválido.'),
  obraSocial: z.string().min(1, 'Requerido.'),
})

export type PacienteFormValues = z.infer<typeof pacienteSchema>
