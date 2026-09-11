import { PageHeader } from '../../../shared/components/PageHeader'
import { DataTable, type Column } from '../../../shared/components/DataTable'
import { useProfesionalesQuery } from '../hooks/useProfesionalesQuery'
import type { ProfesionalDto } from '../types'

const columns: Column<ProfesionalDto>[] = [
  { header: 'Nombre', cell: (p) => `${p.nombre} ${p.apellido}` },
  { header: 'Especialidad', cell: (p) => p.especialidad },
]

export function ProfesionalesPage() {
  const { data, isLoading } = useProfesionalesQuery({ page: 1, pageSize: 20 })

  return (
    <>
      <PageHeader title="Profesionales" />
      <DataTable
        columns={columns}
        rows={data.items}
        isLoading={isLoading}
        getRowId={(p) => p.id}
        emptyMessage="Todavía no hay profesionales."
      />
    </>
  )
}
