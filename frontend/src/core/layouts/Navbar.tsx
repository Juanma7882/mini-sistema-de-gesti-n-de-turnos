import { NavLink, useNavigate } from 'react-router-dom'
import type { Role } from '../auth/session'
import { useAuth } from '../auth/useAuth'
import { InitialsBadge } from './InitialsBadge'

const LINKS: { to: string; label: string; roles: Role[] }[] = [
  { to: '/turnos', label: 'Turnos', roles: ['Admin', 'Profesional'] },
  { to: '/pacientes', label: 'Pacientes', roles: ['Admin'] },
  { to: '/profesionales', label: 'Profesionales', roles: ['Admin'] },
]

export function Navbar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const onLogout = async () => {
    await logout()
    navigate('/login', { replace: true })
  }

  const links = user ? LINKS.filter((link) => link.roles.includes(user.role)) : []

  return (
    <header className="flex items-center justify-between border-b px-6 py-3">
      <nav className="flex gap-4 text-sm">
        {links.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            className={({ isActive }) => (isActive ? 'font-semibold' : 'text-muted-foreground')}
          >
            {link.to === '/turnos' && user?.role === 'Profesional' ? 'Mis turnos' : link.label}
          </NavLink>
        ))}
      </nav>
      <div className="flex items-center gap-3 text-sm">
        {user && (
          <>
            <InitialsBadge nombre={user.nombre} />
            <span>
              {user.nombre} · <span className="text-muted-foreground">{user.role}</span>
            </span>
          </>
        )}
        <button onClick={onLogout} className="text-muted-foreground hover:text-foreground">
          Salir
        </button>
      </div>
    </header>
  )
}
