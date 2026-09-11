import { Compass } from 'lucide-react'
import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="grid min-h-[60vh] place-items-center text-center">
      <div className="flex flex-col items-center gap-3">
        <span className="grid size-11 place-items-center rounded-[10px] bg-primary-tint text-primary">
          <Compass size={22} aria-hidden="true" />
        </span>
        <p className="text-sm text-muted-foreground">No encontramos esta página.</p>
        <Link to="/turnos" className="text-sm font-medium text-primary-strong underline">
          Volver al inicio
        </Link>
      </div>
    </div>
  )
}
