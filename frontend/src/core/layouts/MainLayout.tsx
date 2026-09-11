import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { CalendarDays, HeartPulse, Menu, Stethoscope, Users, X, type LucideIcon } from 'lucide-react'
import type { Role } from '../auth/session'
import { useAuth } from '../auth/useAuth'
import { Navbar } from './Navbar'

interface NavItem {
  to: string
  label: string
  icon: LucideIcon
  roles: Role[]
}

const LINKS: NavItem[] = [
  { to: '/turnos', label: 'Turnos', icon: CalendarDays, roles: ['Admin', 'Profesional'] },
  { to: '/pacientes', label: 'Pacientes', icon: Users, roles: ['Admin'] },
  { to: '/profesionales', label: 'Profesionales', icon: Stethoscope, roles: ['Admin'] },
]

function Brand() {
  return (
    <div className="flex items-center gap-2 px-2">
      <span className="grid size-9 place-items-center rounded-[10px] bg-primary-tint text-primary">
        <HeartPulse size={18} aria-hidden="true" />
      </span>
      <span className="text-sm font-semibold text-foreground">MAE Turnos</span>
    </div>
  )
}

function NavLinks({ links, role, onNavigate }: { links: NavItem[]; role?: Role; onNavigate?: () => void }) {
  return (
    <nav className="flex flex-col gap-1">
      {links.map(({ to, label, icon: Icon }) => (
        <NavLink
          key={to}
          to={to}
          onClick={onNavigate}
          className={({ isActive }) =>
            `flex items-center gap-2.5 rounded-lg border-l-[3px] px-3 py-2 text-sm ${
              isActive
                ? 'border-primary bg-primary-tint font-semibold text-primary-strong'
                : 'border-transparent text-muted-foreground hover:bg-primary-tint/60 hover:text-foreground'
            }`
          }
        >
          <Icon size={17} aria-hidden="true" />
          {to === '/turnos' && role === 'Profesional' ? 'Mis turnos' : label}
        </NavLink>
      ))}
    </nav>
  )
}

export function MainLayout() {
  const { user } = useAuth()
  const [menuOpen, setMenuOpen] = useState(false)
  const links = user ? LINKS.filter((link) => link.roles.includes(user.role)) : []

  return (
    <div className="min-h-dvh bg-canvas md:flex">
      {/* Sidebar de escritorio */}
      <aside className="hidden w-60 shrink-0 flex-col border-r border-border bg-surface py-5 md:flex">
        <div className="flex flex-1 flex-col gap-6 px-3">
          <Brand />
          <NavLinks links={links} role={user?.role} />
        </div>
        <div className="border-t border-border px-3 pt-4">
          <Navbar />
        </div>
      </aside>

      {/* Topbar + panel deslizable en mobile */}
      <div className="flex flex-1 flex-col">
        <header className="flex items-center justify-between border-b border-border bg-surface px-4 py-3 md:hidden">
          <Brand />
          <button
            type="button"
            onClick={() => setMenuOpen(true)}
            aria-label="Abrir menú"
            className="rounded-lg p-2 text-muted-foreground hover:bg-primary-tint hover:text-primary-strong"
          >
            <Menu size={20} aria-hidden="true" />
          </button>
        </header>

        {menuOpen && (
          <div className="fixed inset-0 z-50 md:hidden">
            <button
              type="button"
              aria-label="Cerrar menú"
              onClick={() => setMenuOpen(false)}
              className="absolute inset-0 bg-foreground/30"
            />
            <div
              role="dialog"
              aria-modal="true"
              aria-label="Menú de navegación"
              className="relative flex h-dvh w-72 max-w-[80vw] flex-col gap-6 bg-surface p-4 shadow-[0_0_24px_-4px_rgba(162,60,99,0.35)] motion-reduce:transition-none"
            >
              <div className="flex items-center justify-between">
                <Brand />
                <button
                  type="button"
                  onClick={() => setMenuOpen(false)}
                  aria-label="Cerrar menú"
                  className="rounded-lg p-2 text-muted-foreground hover:bg-primary-tint hover:text-primary-strong"
                >
                  <X size={20} aria-hidden="true" />
                </button>
              </div>
              <NavLinks links={links} role={user?.role} onNavigate={() => setMenuOpen(false)} />
              <div className="mt-auto border-t border-border pt-4">
                <Navbar />
              </div>
            </div>
          </div>
        )}

        <main className="mx-auto w-full max-w-300 flex-1 px-6 py-6 sm:px-8 sm:py-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
