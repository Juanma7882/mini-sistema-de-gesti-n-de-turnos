import { PageHeader } from '../../../shared/components/PageHeader'
import { DataTable, type Column } from '../../../shared/components/DataTable'
import { usePacientesQuery } from '../hooks/usePacientesQuery'
import type { PacienteDto } from '../types'

const columns: Column<PacienteDto>[] = [
  { header: 'Nombre', cell: (p) => `${p.nombre} ${p.apellido}` },
  { header: 'Teléfono', cell: (p) => p.telefono },
  { header: 'Obra social', cell: (p) => p.obraSocial },
]

export function PacientesPage() {
  const { data, isLoading } = usePacientesQuery({ page: 1, pageSize: 20 })

  return (
    <>
      <PageHeader title="Pacientes" />
      <DataTable
        columns={columns}
        rows={data.items}
        isLoading={isLoading}
        getRowId={(p) => p.id}
        emptyMessage="Todavía no hay pacientes."
      />
    </>
  )
}
