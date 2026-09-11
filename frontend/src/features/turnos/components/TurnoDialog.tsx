import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { AlertCircle, CalendarX, Loader2, Search, type LucideIcon } from 'lucide-react'
import { Dialog } from '../../../shared/components/Dialog'
import { FieldError } from '../../../shared/components/FieldError'
import { useProblemForm } from '../../../shared/hooks/useProblemForm'
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue'
import { ApiError } from '../../../core/api/httpClient'
import { pacientesApi } from '../../pacientes/api/pacientesApi'
import { profesionalesApi } from '../../profesionales/api/profesionalesApi'
import type { ProfesionalDto } from '../../profesionales/types'
import { turnosApi } from '../api/turnosApi'
import { turnoSchema, type TurnoFormValues } from '../schemas/turnoSchema'
import type { TurnoDto } from '../types'

interface TurnoDialogProps {
  open: boolean
  /** `null` = alta; con valor = edición. */
  turno: TurnoDto | null
  onClose: () => void
  onSaved: () => void
}

interface PacienteOption {
  id: string
  nombre: string
  apellido: string
}

function splitInicio(inicio: string): { fecha: string; hora: string } {
  const [fecha = '', horaCompleta = ''] = inicio.split('T')
  return { fecha, hora: horaCompleta.slice(0, 5) }
}

/** Fecha de hoy en horario local como YYYY-MM-DD, para el `min` del input de fecha. */
function hoyLocal(): string {
  const d = new Date()
  const mes = String(d.getMonth() + 1).padStart(2, '0')
  const dia = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${mes}-${dia}`
}

/** Hora actual local como HH:MM, para el `min` del input de hora cuando la fecha es hoy. */
function horaActualLocal(): string {
  const d = new Date()
  return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

export function TurnoDialog({ open, turno, onClose, onSaved }: TurnoDialogProps) {
  const [formError, setFormError] = useState<{ message: string; icon: LucideIcon } | null>(null)
  const [fecha, setFecha] = useState('')
  const [hora, setHora] = useState('')

  const [pacienteSearch, setPacienteSearch] = useState('')
  const debouncedPacienteSearch = useDebouncedValue(pacienteSearch)
  const [pacienteOptions, setPacienteOptions] = useState<PacienteOption[]>([])
  const [pacienteDropdownOpen, setPacienteDropdownOpen] = useState(false)
  const [profesionalOptions, setProfesionalOptions] = useState<ProfesionalDto[]>([])

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    setError,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<TurnoFormValues>({ resolver: zodResolver(turnoSchema) })

  const applyProblem = useProblemForm<TurnoFormValues>(setError)
  const pacienteId = watch('pacienteId')

  useEffect(() => {
    if (!open) return
    setFormError(null)
    if (turno) {
      const { fecha: f, hora: h } = splitInicio(turno.inicio)
      setFecha(f)
      setHora(h)
      setPacienteSearch(`${turno.paciente.nombre} ${turno.paciente.apellido}`)
      reset({
        pacienteId: turno.paciente.id,
        profesionalId: turno.profesional.id,
        inicio: turno.inicio,
        notas: turno.notas ?? '',
      })
      setPacienteOptions([turno.paciente])
    } else {
      setFecha('')
      setHora('')
      setPacienteSearch('')
      setPacienteOptions([])
      reset({ pacienteId: '', profesionalId: '', inicio: '', notas: '' })
    }
  }, [open, turno, reset])

  useEffect(() => {
    if (!open) return
    profesionalesApi.listar({ pageSize: 100 }).then((page) => setProfesionalOptions(page.items))
  }, [open])

  useEffect(() => {
    if (!open) return
    pacientesApi
      .listar({ search: debouncedPacienteSearch || undefined, pageSize: 10 })
      .then((page) => setPacienteOptions(page.items))
  }, [open, debouncedPacienteSearch])

  useEffect(() => {
    setValue('inicio', fecha && hora ? `${fecha}T${hora}:00` : '', { shouldValidate: false })
  }, [fecha, hora, setValue])

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null)
    try {
      if (turno) {
        await turnosApi.editar(turno.id, values)
      } else {
        await turnosApi.crear(values)
      }
      onSaved()
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setFormError({ message: err.problem.detail, icon: CalendarX })
        return
      }
      if (err instanceof ApiError && err.status === 404) {
        setFormError({
          message: 'El paciente o profesional seleccionado ya no existe.',
          icon: AlertCircle,
        })
        return
      }
      const detail = applyProblem(err)
      if (detail) setFormError({ message: detail, icon: AlertCircle })
    }
  })

  const Icon = formError?.icon

  return (
    <Dialog open={open} title={turno ? 'Editar turno' : 'Nuevo turno'} onClose={onClose}>
      <form onSubmit={onSubmit} noValidate className="space-y-4">
        {formError && Icon && (
          <p
            role="alert"
            className="flex items-center gap-2 rounded-[10px] bg-primary-tint px-3 py-2 text-sm text-destructive"
          >
            <Icon size={16} className="shrink-0" aria-hidden="true" />
            {formError.message}
          </p>
        )}

        <div>
          <label className="text-sm font-medium" htmlFor="pacienteSearch">
            Paciente
          </label>
          <div className="relative mt-1">
            <Search
              size={16}
              className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
              aria-hidden="true"
            />
            <input
              id="pacienteSearch"
              role="combobox"
              aria-expanded={pacienteDropdownOpen}
              aria-controls="pacienteOptions"
              autoComplete="off"
              value={pacienteSearch}
              onChange={(e) => {
                setPacienteSearch(e.target.value)
                setValue('pacienteId', '', { shouldValidate: false })
                setPacienteDropdownOpen(true)
              }}
              onFocus={() => setPacienteDropdownOpen(true)}
              /* El timeout deja que el click en una opción (onMouseDown) se
                 procese antes de que el blur cierre la lista. */
              onBlur={() => setTimeout(() => setPacienteDropdownOpen(false), 150)}
              placeholder="Buscar paciente…"
              className="w-full rounded-[10px] border border-border py-2 pl-9 pr-3 text-sm outline-none"
            />
            {pacienteDropdownOpen && (
              <ul
                id="pacienteOptions"
                role="listbox"
                className="absolute z-10 mt-1 max-h-48 w-full overflow-auto rounded-[10px] border border-border bg-background shadow-md"
              >
                {pacienteOptions.length === 0 && (
                  <li className="px-3 py-2 text-sm text-muted-foreground">
                    Sin resultados{pacienteSearch ? ` para "${pacienteSearch}"` : ''}.
                  </li>
                )}
                {pacienteOptions.map((p) => (
                  <li
                    key={p.id}
                    role="option"
                    aria-selected={p.id === pacienteId}
                    onMouseDown={(e) => {
                      e.preventDefault()
                      setValue('pacienteId', p.id, { shouldValidate: true })
                      setPacienteSearch(`${p.nombre} ${p.apellido}`)
                      setPacienteDropdownOpen(false)
                    }}
                    className="cursor-pointer px-3 py-2 text-sm hover:bg-primary-tint"
                  >
                    {p.nombre} {p.apellido}
                  </li>
                ))}
              </ul>
            )}
          </div>
          <FieldError message={errors.pacienteId?.message} />
        </div>

        <div>
          <label className="text-sm font-medium" htmlFor="profesionalId">
            Profesional
          </label>
          <select
            id="profesionalId"
            className="mt-1 w-full rounded-[10px] border border-border px-3 py-2 text-sm outline-none"
            {...register('profesionalId')}
          >
            <option value="">Elegí un profesional…</option>
            {profesionalOptions.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nombre} {p.apellido} · {p.especialidad}
              </option>
            ))}
          </select>
          <FieldError message={errors.profesionalId?.message} />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="text-sm font-medium" htmlFor="fecha">
              Fecha
            </label>
            <input
              id="fecha"
              type="date"
              min={hoyLocal()}
              value={fecha}
              onChange={(e) => {
                if (e.target.value && e.target.value < hoyLocal()) return
                setFecha(e.target.value)
              }}
              className="mt-1 w-full rounded-[10px] border border-border px-3 py-2 text-sm outline-none"
            />
          </div>
          <div>
            <label className="text-sm font-medium" htmlFor="hora">
              Hora
            </label>
            <input
              id="hora"
              type="time"
              min={fecha === hoyLocal() ? horaActualLocal() : undefined}
              value={hora}
              onChange={(e) => {
                if (e.target.value && fecha === hoyLocal() && e.target.value < horaActualLocal()) return
                setHora(e.target.value)
              }}
              className="mt-1 w-full rounded-[10px] border border-border px-3 py-2 text-sm outline-none"
            />
          </div>
        </div>
        <FieldError message={errors.inicio?.message} />

        <div>
          <label className="text-sm font-medium" htmlFor="notas">
            Notas
          </label>
          <textarea
            id="notas"
            rows={3}
            className="mt-1 w-full rounded-[10px] border border-border px-3 py-2 text-sm outline-none"
            {...register('notas')}
          />
          <FieldError message={errors.notas?.message} />
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
