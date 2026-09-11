import { Navigate, Outlet } from 'react-router-dom'
import { FullScreenSpinner } from '../../shared/components/FullScreenSpinner'
import { useAuth } from './useAuth'

/** Guard de rutas públicas (ej. /login): con sesión activa → /turnos, sin flash del formulario. */
export function RedirectIfAuthenticated() {
  const { status } = useAuth()

  if (status === 'loading') {
    return <FullScreenSpinner />
  }
  if (status === 'authenticated') {
    return <Navigate to="/turnos" replace />
  }
  return <Outlet />
}
