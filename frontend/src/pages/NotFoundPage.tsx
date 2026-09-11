import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="grid min-h-dvh place-items-center text-center">
      <div>
        <p className="text-2xl font-semibold">404</p>
        <p className="mt-2 text-sm text-muted-foreground">La página no existe.</p>
        <Link to="/turnos" className="mt-4 inline-block text-sm underline">
          Volver al inicio
        </Link>
      </div>
    </div>
  )
}
