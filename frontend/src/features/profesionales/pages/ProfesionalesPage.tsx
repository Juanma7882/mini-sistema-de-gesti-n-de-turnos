import { useState } from 'react'
import { Pencil, Plus, Search, Stethoscope, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { PageHeader } from '../../../shared/components/PageHeader'
import { DataTable, type Column } from '../../../shared/components/DataTable'
import { EmptyState } from '../../../shared/components/EmptyState'
import { ConfirmDialog } from '../../../shared/components/ConfirmDialog'
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue'
import { ApiError } from '../../../core/api/httpClient'
import { useProfesionalesQuery } from '../hooks/useProfesionalesQuery'
import { profesionalesApi } from '../api/profesionalesApi'
import { ProfesionalDialog } from '../components/ProfesionalDialog'
import type { ProfesionalDto } from '../types'

export function ProfesionalesPage() {
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search)
  const { data, isLoading, refetch } = useProfesionalesQuery({
    search: debouncedSearch || undefined,
    page: 1,
    pageSize: 20,
  })

  const [editing, setEditing] = useState<ProfesionalDto | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [deleting, setDeleting] = useState<ProfesionalDto | null>(null)

  const columns: Column<ProfesionalDto>[] = [
    { header: 'Nombre', cell: (p) => `${p.nombre} ${p.apellido}` },
    { header: 'Especialidad', cell: (p) => p.especialidad },
    {
      header: 'Acciones',
      cell: (p) => (
        <div className="flex items-center gap-1">
          <button
            type="button"
            aria-label="Editar profesional"
            onClick={() => {
              setEditing(p)
              setDialogOpen(true)
            }}
            className="rounded-lg p-1.5 text-muted-foreground hover:bg-primary-tint hover:text-primary-strong"
          >
            <Pencil size={15} aria-hidden="true" />
          </button>
          <button
            type="button"
            aria-label="Eliminar profesional"
            onClick={() => setDeleting(p)}
            className="rounded-lg p-1.5 text-muted-foreground hover:bg-primary-tint hover:text-destructive"
          >
            <Trash2 size={15} aria-hidden="true" />
          </button>
        </div>
      ),
    },
  ]

  const emptyMessage = search ? (
    <EmptyState>
      <div className="flex flex-col items-center gap-2">
        <Stethoscope size={28} className="text-muted-foreground" aria-hidden="true" />
        <p>No encontramos profesionales con ese texto.</p>
      </div>
    </EmptyState>
  ) : (
    <EmptyState>
      <div className="flex flex-col items-center gap-3">
        <Stethoscope size={28} className="text-muted-foreground" aria-hidden="true" />
        <p>Todavía no hay profesionales.</p>
        <button
          type="button"
          onClick={() => {
            setEditing(null)
            setDialogOpen(true)
          }}
          className="rounded-lg bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground"
        >
          Nuevo profesional
        </button>
      </div>
    </EmptyState>
  )

  const onDelete = async () => {
    if (!deleting) return
    try {
      await profesionalesApi.eliminar(deleting.id)
      toast.success('Profesional eliminado')
      setDeleting(null)
      void refetch()
    } catch (err) {
      setDeleting(null)
      if (err instanceof ApiError && err.status === 409) {
        toast.error('Este profesional tiene turnos activos. Cancelá o reasigná esos turnos primero.')
      }
    }
  }

  return (
    <>
      <PageHeader
        title="Profesionales"
        actions={
          <button
            type="button"
            onClick={() => {
              setEditing(null)
              setDialogOpen(true)
            }}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground"
          >
            <Plus size={16} aria-hidden="true" />
            Nuevo profesional
          </button>
        }
      />

      <div className="relative mb-4 max-w-xs">
        <Search
          size={16}
          className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
          aria-hidden="true"
        />
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Buscar profesional…"
          className="w-full rounded-[10px] border border-border py-2 pl-9 pr-3 text-sm outline-none"
        />
      </div>

      <DataTable
        columns={columns}
        rows={data.items}
        isLoading={isLoading}
        getRowId={(p) => p.id}
        emptyMessage={emptyMessage}
      />

      <ProfesionalDialog
        open={dialogOpen}
        profesional={editing}
        onClose={() => setDialogOpen(false)}
        onSaved={() => {
          setDialogOpen(false)
          toast.success(editing ? 'Profesional actualizado' : 'Profesional creado')
          void refetch()
        }}
      />

      <ConfirmDialog
        open={deleting !== null}
        title="Eliminar profesional"
        confirmLabel="Eliminar"
        icon={Trash2}
        onConfirm={onDelete}
        onCancel={() => setDeleting(null)}
      />
    </>
  )
}
