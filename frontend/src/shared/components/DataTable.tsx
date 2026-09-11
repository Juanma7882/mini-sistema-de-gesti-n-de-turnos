import type { ReactNode } from 'react'
import { EmptyState } from './EmptyState'

export interface Column<T> {
  header: string
  cell: (row: T) => ReactNode
}

interface DataTableProps<T> {
  columns: Column<T>[]
  rows: T[]
  getRowId: (row: T) => string
  isLoading?: boolean
  onRowClick?: (row: T) => void
  emptyMessage?: string
}

// TODO(frontend.md §4.3): paginación + skeletons con shadcn/ui <Table>.
export function DataTable<T>({
  columns,
  rows,
  getRowId,
  isLoading,
  onRowClick,
  emptyMessage = 'Sin resultados.',
}: DataTableProps<T>) {
  if (isLoading) {
    return <div className="py-12 text-center text-sm text-muted-foreground">Cargando…</div>
  }
  if (rows.length === 0) {
    return <EmptyState>{emptyMessage}</EmptyState>
  }

  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full text-sm">
        <thead className="border-b bg-muted/50 text-left">
          <tr>
            {columns.map((col) => (
              <th key={col.header} className="px-4 py-2 font-medium">
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr
              key={getRowId(row)}
              onClick={onRowClick ? () => onRowClick(row) : undefined}
              className={onRowClick ? 'cursor-pointer border-b hover:bg-muted/30' : 'border-b'}
            >
              {columns.map((col) => (
                <td key={col.header} className="px-4 py-2">
                  {col.cell(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
