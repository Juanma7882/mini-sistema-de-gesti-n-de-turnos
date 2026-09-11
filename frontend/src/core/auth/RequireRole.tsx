import { Navigate, Outlet } from 'react-router-dom'
import type { Role } from './session'
import { useAuth } from './useAuth'

/** Guard de ruta: rol incorrecto → /403. El gating por rol es solo UX. */
export function RequireRole({ roles }: { roles: Role[] }) {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  if (!roles.includes(user.role)) return <Navigate to="/403" replace />
  return <Outlet />
}
