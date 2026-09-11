import { PageHeader } from '../../../shared/components/PageHeader'
import { DataTable, type Column } from '../../../shared/components/DataTable'
import { formatInicio } from '../../../shared/lib/formatInicio'
import { useAuth } from '../../../core/auth/useAuth'
import { useTurnosQuery } from '../hooks/useTurnosQuery'
import type { TurnoDto } from '../types'

const columns: Column<TurnoDto>[] = [
  { header: 'Paciente', cell: (t) => `${t.paciente.nombre} ${t.paciente.apellido}` },
  { header: 'Profesional', cell: (t) => `${t.profesional.nombre} ${t.profesional.apellido}` },
  { header: 'Inicio', cell: (t) => formatInicio(t.inicio) },
  { header: 'Estado', cell: (t) => t.estado },
  { header: 'Notas', cell: (t) => t.notas ?? '—' },
]

export function TurnosPage() {
  const { user } = useAuth()
  const { data, isLoading } = useTurnosQuery({ page: 1, pageSize: 20 })

  return (
    <>
      <PageHeader title={user?.role === 'Profesional' ? 'Mis turnos' : 'Turnos'} />
      <DataTable
        columns={columns}
        rows={data.items}
        isLoading={isLoading}
        getRowId={(t) => t.id}
        emptyMessage="No hay turnos para los filtros actuales."
      />
    </>
  )
}
