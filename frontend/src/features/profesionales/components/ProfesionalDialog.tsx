import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { AlertCircle, Loader2 } from 'lucide-react'
import { Dialog } from '../../../shared/components/Dialog'
import { FieldError } from '../../../shared/components/FieldError'
import { useProblemForm } from '../../../shared/hooks/useProblemForm'
import { profesionalesApi } from '../api/profesionalesApi'
import { profesionalSchema, type ProfesionalFormValues } from '../schemas/profesionalSchema'
import type { ProfesionalDto } from '../types'

interface ProfesionalDialogProps {
  open: boolean
  /** `null` = alta; con valor = edición. */
  profesional: ProfesionalDto | null
  onClose: () => void
  onSaved: () => void
}

const EMPTY: ProfesionalFormValues = { nombre: '', apellido: '', especialidad: '' }

export function ProfesionalDialog({ open, profesional, onClose, onSaved }: ProfesionalDialogProps) {
  const [formError, setFormError] = useState<string | null>(null)
  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<ProfesionalFormValues>({ resolver: zodResolver(profesionalSchema), defaultValues: EMPTY })

  const applyProblem = useProblemForm<ProfesionalFormValues>(setError)

  useEffect(() => {
    if (open) {
      setFormError(null)
      reset(
        profesional
          ? { nombre: profesional.nombre, apellido: profesional.apellido, especialidad: profesional.especialidad }
          : EMPTY,
      )
    }
  }, [open, profesional, reset])

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null)
    try {
      if (profesional) {
        await profesionalesApi.editar(profesional.id, values)
      } else {
        await profesionalesApi.crear(values)
      }
      onSaved()
    } catch (err) {
      const detail = applyProblem(err)
      if (detail) setFormError(detail)
    }
  })

  return (
    <Dialog open={open} title={profesional ? 'Editar profesional' : 'Nuevo profesional'} onClose={onClose}>
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
          <label className="text-sm font-medium" htmlFor="especialidad">
            Especialidad
          </label>
          <input
            id="especialidad"
            className="mt-1 w-full rounded-[10px] border border-border px-3 py-2 text-sm outline-none"
            {...register('especialidad')}
          />
          <FieldError message={errors.especialidad?.message} />
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
