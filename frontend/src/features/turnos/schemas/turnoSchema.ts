import { z } from 'zod'

export const turnoSchema = z.object({
  pacienteId: z.string().min(1, 'Elegí un paciente.'),
  profesionalId: z.string().min(1, 'Elegí un profesional.'),
  inicio: z.string().min(1, 'Elegí fecha y hora.'),
  notas: z.string().max(500).optional(),
})

export type TurnoFormValues = z.infer<typeof turnoSchema>
