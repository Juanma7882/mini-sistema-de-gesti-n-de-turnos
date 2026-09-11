import { LogOut } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { InitialsBadge } from './InitialsBadge'

/** Identidad del usuario + logout (frontend-app-shell spec, "Navbar con identidad de usuario y logout"). */
export function Navbar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  if (!user) return null

  const onLogout = async () => {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="flex rounded-full w-full items-center gap-2.5 font-poppins">
      <InitialsBadge nombre={user.nombre} />
      <div className="flex min-w-0 flex-1 flex-col leading-tight">
        <span className="truncate text-sm font-medium text-foreground">{user.nombre}</span>
        <span className="w-fit rounded-md bg-primary-tint px-1.5 py-0.5 text-meta font-medium text-primary-strong">
          {user.role}
        </span>
      </div>
      <button
        type="button"
        onClick={onLogout}
        aria-label="Cerrar sesión"
        className="shrink-0 rounded-full p-2 text-muted-foreground hover:bg-primary-tint hover:text-primary-strong"
      >
        <LogOut size={16} aria-hidden="true" />
      </button>
    </div>
  )
}
