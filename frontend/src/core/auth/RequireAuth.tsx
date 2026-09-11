import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { FullScreenSpinner } from '../../shared/components/FullScreenSpinner'
import { useAuth } from './useAuth'

/** Guard de ruta: sin sesión → /login (guardando `from`). */
export function RequireAuth() {
  const { status } = useAuth()
  const location = useLocation()

  if (status === 'loading') {
    return <FullScreenSpinner />
  }
  if (status === 'anonymous') {
    return <Navigate to="/login" replace state={{ from: location }} />
  }
  return <Outlet />
}
