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
  emptyMessage?: ReactNode
}

const SKELETON_ROWS = 6

// TODO(frontend.md §4.3): paginación con un componente de tabla más completo.
export function DataTable<T>({
  columns,
  rows,
  getRowId,
  isLoading,
  onRowClick,
  emptyMessage = 'Sin resultados.',
}: DataTableProps<T>) {
  if (!isLoading && rows.length === 0) {
    return <EmptyState>{emptyMessage}</EmptyState>
  }

  return (
    <div className="overflow-x-auto rounded-[10px] border border-border">
      <table className="w-full text-body">
        <thead className="border-b border-border bg-primary-tint/40 text-left">
          <tr>
            {columns.map((col) => (
              <th key={col.header} className="px-4 py-2 text-table-header font-medium text-foreground">
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {isLoading
            ? Array.from({ length: SKELETON_ROWS }, (_, i) => (
                <tr key={i} className="border-b border-border last:border-b-0">
                  {columns.map((col) => (
                    <td key={col.header} className="px-4 py-2.5">
                      <div
                        className="h-4 w-4/5 animate-pulse rounded bg-primary-tint motion-reduce:animate-none"
                        aria-hidden="true"
                      />
                    </td>
                  ))}
                </tr>
              ))
            : rows.map((row) => (
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
