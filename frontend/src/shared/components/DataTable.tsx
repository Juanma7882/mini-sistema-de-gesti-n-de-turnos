import type { ReactNode } from 'react'
import { Loader2 } from 'lucide-react'
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
  emptyMessage?: ReactNode
}

// TODO(frontend.md §4.3): paginación + skeletons con un componente de tabla más completo.
export function DataTable<T>({
  columns,
  rows,
  getRowId,
  isLoading,
  onRowClick,
  emptyMessage = 'Sin resultados.',
}: DataTableProps<T>) {
  if (isLoading) {
    return (
      <div className="grid place-items-center py-16">
        <Loader2 size={22} className="animate-spin text-primary" aria-hidden="true" />
      </div>
    )
  }
  if (rows.length === 0) {
    return <EmptyState>{emptyMessage}</EmptyState>
  }

  return (
    <div className="overflow-x-auto rounded-[10px] border border-border">
      <table className="w-full text-sm">
        <thead className="border-b border-border bg-primary-tint/40 text-left">
          <tr>
            {columns.map((col) => (
              <th key={col.header} className="px-4 py-2 font-medium text-foreground">
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
              onKeyDown={
                onRowClick
                  ? (e) => {
                      if (e.key === 'Enter') onRowClick(row)
                    }
                  : undefined
              }
              tabIndex={onRowClick ? 0 : undefined}
              className={`border-b border-border last:border-b-0 ${
                onRowClick ? 'cursor-pointer hover:bg-primary-tint' : ''
              }`}
            >
              {columns.map((col) => (
                <td key={col.header} className="px-4 py-2.5">
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
