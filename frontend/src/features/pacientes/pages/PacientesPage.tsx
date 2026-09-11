import { useState } from 'react'
import { Pencil, Plus, Search, Trash2, UserSearch, Users } from 'lucide-react'
import { toast } from 'sonner'
import { PageHeader } from '../../../shared/components/PageHeader'
import { DataTable, type Column } from '../../../shared/components/DataTable'
import { EmptyState } from '../../../shared/components/EmptyState'
import { ConfirmDialog } from '../../../shared/components/ConfirmDialog'
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue'
import { ApiError } from '../../../core/api/httpClient'
import { usePacientesQuery } from '../hooks/usePacientesQuery'
import { pacientesApi } from '../api/pacientesApi'
import { PacienteDialog } from '../components/PacienteDialog'
import type { PacienteDto } from '../types'

export function PacientesPage() {
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search)
  const { data, isLoading, refetch } = usePacientesQuery({
    search: debouncedSearch || undefined,
    page: 1,
    pageSize: 20,
  })

  const [editing, setEditing] = useState<PacienteDto | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [deleting, setDeleting] = useState<PacienteDto | null>(null)

  const columns: Column<PacienteDto>[] = [
    { header: 'Nombre', cell: (p) => `${p.nombre} ${p.apellido}` },
    { header: 'Teléfono', cell: (p) => p.telefono },
    { header: 'Obra social', cell: (p) => p.obraSocial },
    {
      header: 'Acciones',
      cell: (p) => (
        <div className="flex items-center gap-1">
          <button
            type="button"
            aria-label="Editar paciente"
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
            aria-label="Eliminar paciente"
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
        <UserSearch size={28} className="text-muted-foreground" aria-hidden="true" />
        <p>No encontramos pacientes con ese texto.</p>
      </div>
    </EmptyState>
  ) : (
    <EmptyState>
      <div className="flex flex-col items-center gap-3">
        <Users size={28} className="text-muted-foreground" aria-hidden="true" />
        <p>Todavía no hay pacientes.</p>
        <button
          type="button"
          onClick={() => {
            setEditing(null)
            setDialogOpen(true)
          }}
          className="rounded-lg bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground"
        >
          Nuevo paciente
        </button>
      </div>
    </EmptyState>
  )

  const onDelete = async () => {
    if (!deleting) return
    try {
      await pacientesApi.eliminar(deleting.id)
      toast.success('Paciente eliminado')
      setDeleting(null)
      void refetch()
    } catch (err) {
      setDeleting(null)
      if (err instanceof ApiError && err.status === 409) {
        toast.error('Este paciente tiene turnos activos. Cancelá o reasigná esos turnos primero.')
      }
    }
  }

  return (
    <>
      <PageHeader
        title="Pacientes"
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
            Nuevo paciente
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
          placeholder="Buscar paciente…"
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

      <PacienteDialog
        open={dialogOpen}
        paciente={editing}
        onClose={() => setDialogOpen(false)}
        onSaved={() => {
          setDialogOpen(false)
          toast.success(editing ? 'Paciente actualizado' : 'Paciente creado')
          void refetch()
        }}
      />

      <ConfirmDialog
        open={deleting !== null}
        title="Eliminar paciente"
        description="No podrás eliminarlo si tiene turnos activos."
        confirmLabel="Eliminar"
        icon={Trash2}
        onConfirm={onDelete}
        onCancel={() => setDeleting(null)}
      />
    </>
  )
}
