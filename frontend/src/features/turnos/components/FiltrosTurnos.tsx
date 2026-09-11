import { X } from 'lucide-react'
import type { EstadoTurno } from '../../../shared/types/api'
import type { ProfesionalDto } from '../../profesionales/types'

export interface FiltrosTurnosValue {
  desde: string
  hasta: string
  estado: EstadoTurno | ''
  profesionalId: string
}

interface FiltrosTurnosProps {
  value: FiltrosTurnosValue
  onChange: (patch: Partial<FiltrosTurnosValue>) => void
  onClear: () => void
  hasActiveFilters: boolean
  isAdmin: boolean
  profesionales: ProfesionalDto[]
}

const ESTADOS: EstadoTurno[] = ['Pendiente', 'Confirmado', 'Atendido', 'Cancelado']

/** Rango de fechas, estado y (solo Admin) profesional; sincronizados a la query string por el caller. */
export function FiltrosTurnos({
  value,
  onChange,
  onClear,
  hasActiveFilters,
  isAdmin,
  profesionales,
}: FiltrosTurnosProps) {
  return (
    <div className="mb-4 flex flex-wrap items-end gap-3">
      <div>
        <label className="text-xs font-medium text-muted-foreground" htmlFor="desde">
          Desde
        </label>
        <input
          id="desde"
          type="date"
          value={value.desde}
          onChange={(e) => onChange({ desde: e.target.value })}
          className="mt-1 rounded-[10px] border border-border px-3 py-1.5 text-sm outline-none"
        />
      </div>
      <div>
        <label className="text-xs font-medium text-muted-foreground" htmlFor="hasta">
          Hasta
        </label>
        <input
          id="hasta"
          type="date"
          value={value.hasta}
          onChange={(e) => onChange({ hasta: e.target.value })}
          className="mt-1 rounded-[10px] border border-border px-3 py-1.5 text-sm outline-none"
        />
      </div>
      <div>
        <label className="text-xs font-medium text-muted-foreground" htmlFor="estado">
          Estado
        </label>
        <select
          id="estado"
          value={value.estado}
          onChange={(e) => onChange({ estado: e.target.value as EstadoTurno | '' })}
          className="mt-1 rounded-[10px] border border-border px-3 py-1.5 text-sm outline-none"
        >
          <option value="">Todos</option>
          {ESTADOS.map((estado) => (
            <option key={estado} value={estado}>
              {estado}
            </option>
          ))}
        </select>
      </div>
      {isAdmin && (
        <div>
          <label className="text-xs font-medium text-muted-foreground" htmlFor="profesionalId">
            Profesional
          </label>
          <select
            id="profesionalId"
            value={value.profesionalId}
            onChange={(e) => onChange({ profesionalId: e.target.value })}
            className="mt-1 rounded-[10px] border border-border px-3 py-1.5 text-sm outline-none"
          >
            <option value="">Todos</option>
            {profesionales.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nombre} {p.apellido}
              </option>
            ))}
          </select>
        </div>
      )}
      {hasActiveFilters && (
        <button
          type="button"
          onClick={onClear}
          className="flex items-center gap-1 rounded-lg px-2 py-1.5 text-sm text-muted-foreground hover:bg-primary-tint hover:text-primary-strong"
        >
          <X size={14} aria-hidden="true" />
          Limpiar filtros
        </button>
      )}
    </div>
  )
}
