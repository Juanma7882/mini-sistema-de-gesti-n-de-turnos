import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { AlertTriangle, CalendarSearch, ChevronRight, Plus } from 'lucide-react'
import { toast } from 'sonner'
import { PageHeader } from '../../../shared/components/PageHeader'
import { DataTable, type Column } from '../../../shared/components/DataTable'
import { EmptyState } from '../../../shared/components/EmptyState'
import { formatInicio } from '../../../shared/lib/formatInicio'
import { useAuth } from '../../../core/auth/useAuth'
import { turnosHub } from '../../../core/realtime/turnosHub'
import { useProfesionalesQuery } from '../../profesionales/hooks/useProfesionalesQuery'
import { useTurnosQuery } from '../hooks/useTurnosQuery'
import { EstadoBadge } from '../components/EstadoBadge'
import { FiltrosTurnos, type FiltrosTurnosValue } from '../components/FiltrosTurnos'
import { TurnoDetailDrawer } from '../components/TurnoDetailDrawer'
import { TurnoDialog } from '../components/TurnoDialog'
import type { TurnoDto } from '../types'

const EMPTY_FILTERS: FiltrosTurnosValue = { desde: '', hasta: '', estado: '', profesionalId: '' }

export function TurnosPage() {
  const { user } = useAuth()
  const isAdmin = user?.role === 'Admin'
  const [searchParams, setSearchParams] = useSearchParams()

  const filters: FiltrosTurnosValue = {
    desde: searchParams.get('desde') ?? '',
    hasta: searchParams.get('hasta') ?? '',
    estado: (searchParams.get('estado') as FiltrosTurnosValue['estado']) ?? '',
    profesionalId: searchParams.get('profesionalId') ?? '',
  }
  const hasActiveFilters = Object.values(filters).some(Boolean)

  const onFiltersChange = (patch: Partial<FiltrosTurnosValue>) => {
    const next = new URLSearchParams(searchParams)
    for (const [key, val] of Object.entries(patch)) {
      if (val) next.set(key, val)
      else next.delete(key)
    }
    setSearchParams(next, { replace: true })
  }

  const onClearFilters = () => {
    const next = new URLSearchParams(searchParams)
    for (const key of Object.keys(EMPTY_FILTERS)) next.delete(key)
    setSearchParams(next, { replace: true })
  }

  const { data: profesionalesPage } = useProfesionalesQuery({ pageSize: 100 })

  const { data, isLoading, error, refetch } = useTurnosQuery({
    desde: filters.desde || undefined,
    hasta: filters.hasta || undefined,
    estado: filters.estado || undefined,
    profesionalId: isAdmin ? filters.profesionalId || undefined : undefined,
    page: 1,
    pageSize: 20,
  })

  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingTurno, setEditingTurno] = useState<TurnoDto | null>(null)

  // Un turno creado/editado/cambiado de estado en otra sesión (ej. el Admin le
  // asigna un turno a un Profesional logueado en otra pestaña) llega acá y
  // refresca el listado sin recargar la página.
  useEffect(() => turnosHub.subscribe(() => void refetch()), [refetch])

  const openDetail = (turno: TurnoDto) => {
    const next = new URLSearchParams(searchParams)
    next.set('turno', turno.id)
    setSearchParams(next, { replace: true })
  }

  const columns: Column<TurnoDto>[] = [
    { header: 'Paciente', cell: (t) => `${t.paciente.nombre} ${t.paciente.apellido}` },
    ...(isAdmin
      ? [
          {
            header: 'Profesional',
            cell: (t: TurnoDto) => `${t.profesional.nombre} ${t.profesional.apellido}`,
          },
        ]
      : []),
    { header: 'Inicio', cell: (t) => <span className="tabular-nums">{formatInicio(t.inicio)}</span> },
    { header: 'Estado', cell: (t) => <EstadoBadge estado={t.estado} /> },
    { header: 'Notas', cell: (t) => <span className="line-clamp-1 max-w-50">{t.notas ?? '—'}</span> },
    {
      header: '',
      cell: () => <ChevronRight size={16} className="text-muted-foreground" aria-hidden="true" />,
    },
  ]

  const emptyMessage = hasActiveFilters ? (
    <EmptyState>
      <div className="flex flex-col items-center gap-3">
        <CalendarSearch size={28} className="text-muted-foreground" aria-hidden="true" />
        <p>No hay turnos para estos filtros.</p>
        <button
          type="button"
          onClick={onClearFilters}
          className="text-sm font-medium text-primary-strong underline"
        >
          Limpiar filtros
        </button>
      </div>
    </EmptyState>
  ) : (
    <EmptyState>
      <div className="flex flex-col items-center gap-3">
        <CalendarSearch size={28} className="text-muted-foreground" aria-hidden="true" />
        <p>Todavía no hay turnos.</p>
        {isAdmin && (
          <button
            type="button"
            onClick={() => {
              setEditingTurno(null)
              setDialogOpen(true)
            }}
            className="rounded-lg bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground"
          >
            Nuevo turno
          </button>
        )}
      </div>
    </EmptyState>
  )

  return (
    <>
      <PageHeader
        title={user?.role === 'Profesional' ? 'Mis turnos' : 'Turnos'}
        actions={
          isAdmin ? (
            <button
              type="button"
              onClick={() => {
                setEditingTurno(null)
                setDialogOpen(true)
              }}
              className="flex items-center gap-1.5 rounded-lg bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground"
            >
              <Plus size={16} aria-hidden="true" />
              Nuevo turno
            </button>
          ) : undefined
        }
      />

      <FiltrosTurnos
        value={filters}
        onChange={onFiltersChange}
        onClear={onClearFilters}
        hasActiveFilters={hasActiveFilters}
        isAdmin={isAdmin}
        profesionales={profesionalesPage.items}
      />

      {error && error.status < 500 && error.status !== 0 ? (
        <div className="flex flex-col items-center gap-3 rounded-[10px] border border-border py-12 text-center">
          <AlertTriangle size={28} className="text-destructive" aria-hidden="true" />
          <p className="text-sm text-muted-foreground">No pudimos cargar los turnos.</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="rounded-lg border border-border px-3 py-1.5 text-sm text-foreground hover:bg-primary-tint"
          >
            Reintentar
          </button>
        </div>
      ) : (
        <DataTable
          columns={columns}
          rows={data.items}
          isLoading={isLoading}
          getRowId={(t) => t.id}
          onRowClick={openDetail}
          emptyMessage={emptyMessage}
        />
      )}

      <TurnoDetailDrawer
        onEdit={(turno) => {
          setEditingTurno(turno)
          setDialogOpen(true)
        }}
        onEstadoChanged={() => void refetch()}
      />

      <TurnoDialog
        open={dialogOpen}
        turno={editingTurno}
        onClose={() => setDialogOpen(false)}
        onSaved={() => {
          setDialogOpen(false)
          toast.success(editingTurno ? 'Turno actualizado' : 'Turno creado')
          void refetch()
        }}
      />
    </>
  )
}
