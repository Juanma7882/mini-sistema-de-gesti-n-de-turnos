import type { ReactNode } from 'react'

export function EmptyState({ children }: { children: ReactNode }) {
  return <div className="py-12 text-center text-sm text-muted-foreground">{children}</div>
}
