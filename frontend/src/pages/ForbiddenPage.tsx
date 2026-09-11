import { Link } from 'react-router-dom'

export function ForbiddenPage() {
  return (
    <div className="grid min-h-dvh place-items-center text-center">
      <div>
        <p className="text-2xl font-semibold">403</p>
        <p className="mt-2 text-sm text-muted-foreground">No tenés permiso para ver esta sección.</p>
        <Link to="/turnos" className="mt-4 inline-block text-sm underline">
          Volver
        </Link>
      </div>
    </div>
  )
}
