import { ShieldX } from 'lucide-react'
import { Link } from 'react-router-dom'

export function ForbiddenPage() {
  return (
    <div className="grid min-h-[60vh] place-items-center text-center">
      <div className="flex flex-col items-center gap-3">
        <span className="grid size-11 place-items-center rounded-[10px] bg-primary-tint text-primary">
          <ShieldX size={22} aria-hidden="true" />
        </span>
        <p className="text-sm text-muted-foreground">No tenés permiso para ver esta página.</p>
        <Link
          to="/turnos"
          className="rounded-lg bg-primary px-3 py-2 text-sm font-medium text-primary-foreground"
        >
          Ir a turnos
        </Link>
      </div>
    </div>
  )
}
