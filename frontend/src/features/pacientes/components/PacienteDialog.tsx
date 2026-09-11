import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { AlertCircle, Loader2 } from 'lucide-react'
import { Dialog } from '../../../shared/components/Dialog'
import { FieldError } from '../../../shared/components/FieldError'
import { useProblemForm } from '../../../shared/hooks/useProblemForm'
import { pacientesApi } from '../api/pacientesApi'
import { pacienteSchema, type PacienteFormValues } from '../schemas/pacienteSchema'
import type { PacienteDto } from '../types'

interface PacienteDialogProps {
  open: boolean
  /** `null` = alta; con valor = edición. */
  paciente: PacienteDto | null
  onClose: () => void
  onSaved: () => void
}

const EMPTY: PacienteFormValues = { nombre: '', apellido: '', telefono: '', obraSocial: '' }

export function PacienteDialog({ open, paciente, onClose, onSaved }: PacienteDialogProps) {
  const [formError, setFormError] = useState<string | null>(null)
  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<PacienteFormValues>({ resolver: zodResolver(pacienteSchema), defaultValues: EMPTY })

  const applyProblem = useProblemForm<PacienteFormValues>(setError)

  useEffect(() => {
    if (open) {
      setFormError(null)
      reset(
        paciente
          ? {
              nombre: paciente.nombre,
              apellido: paciente.apellido,
              telefono: paciente.telefono,
              obraSocial: paciente.obraSocial,
            }
          : EMPTY,
      )
    }
  }, [open, paciente, reset])

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null)
    try {
      if (paciente) {
        await pacientesApi.editar(paciente.id, values)
      } else {
        await pacientesApi.crear(values)
      }
      onSaved()
    } catch (err) {
      const detail = applyProblem(err)
      if (detail) setFormError(detail)
    }
  })

  return (
    <Dialog open={open} title={paciente ? 'Editar paciente' : 'Nuevo paciente'} onClose={onClose}>
      <form onSubmit={onSubmit} noValidate className="space-y-4">
        {formError && (
          <p
            role="alert"
            className="flex items-center gap-2 rounded-[10px] bg-primary-tint px-3 py-2 text-sm text-destructive"
          >
            <AlertCircle size={16} className="shrink-0" aria-hidden="true" />
            {formError}
          </p>
        )}

        <div>
          <label className="text-sm font-medium" htmlFor="nombre">
            Nombre
          </label>
          <input
            id="nombre"
            className="mt-1 w-full rounded-[10px] border border-border px-3 py-2 text-sm outline-none"
            {...register('nombre')}
          />
          <FieldError message={errors.nombre?.message} />
        </div>

        <div>
          <label className="text-sm font-medium" htmlFor="apellido">
            Apellido
          </label>
          <input
            id="apellido"
            className="mt-1 w-full rounded-[10px] border border-border px-3 py-2 text-sm outline-none"
            {...register('apellido')}
          />
          <FieldError message={errors.apellido?.message} />
        </div>

        <div>
          <label className="text-sm font-medium" htmlFor="telefono">
            Teléfono
          </label>
          <input
            id="telefono"
            className="mt-1 w-full rounded-[10px] border border-border px-3 py-2 text-sm outline-none"
            {...register('telefono')}
          />
          <FieldError message={errors.telefono?.message} />
        </div>

        <div>
          <label className="text-sm font-medium" htmlFor="obraSocial">
            Obra social
          </label>
          <input
            id="obraSocial"
            className="mt-1 w-full rounded-[10px] border border-border px-3 py-2 text-sm outline-none"
            {...register('obraSocial')}
          />
          <FieldError message={errors.obraSocial?.message} />
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg border border-border px-3 py-1.5 text-sm text-foreground hover:bg-primary-tint"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="flex items-center gap-2 rounded-lg bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground disabled:opacity-50"
          >
            {isSubmitting && <Loader2 size={16} className="animate-spin" aria-hidden="true" />}
            {isSubmitting ? 'Guardando' : 'Guardar'}
          </button>
        </div>
      </form>
    </Dialog>
  )
}
