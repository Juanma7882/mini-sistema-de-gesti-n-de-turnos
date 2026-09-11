import { createBrowserRouter, Navigate } from 'react-router-dom'
import { RedirectIfAuthenticated } from '../core/auth/RedirectIfAuthenticated'
import { RequireAuth } from '../core/auth/RequireAuth'
import { RequireRole } from '../core/auth/RequireRole'
import { MainLayout } from '../core/layouts/MainLayout'
import { LoginPage } from '../features/auth/pages/LoginPage'
import { PacientesPage } from '../features/pacientes/pages/PacientesPage'
import { ProfesionalesPage } from '../features/profesionales/pages/ProfesionalesPage'
import { TurnosPage } from '../features/turnos/pages/TurnosPage'
import { ForbiddenPage } from '../pages/ForbiddenPage'
import { NotFoundPage } from '../pages/NotFoundPage'

// Todas las rutas se definen acá; las features no se autorregistran
// (arquitectura-frontend.md §3.6).
export const router = createBrowserRouter([
  {
    element: <RedirectIfAuthenticated />,
    children: [{ path: '/login', element: <LoginPage /> }],
  },
  {
    element: <RequireAuth />,
    children: [
      {
        element: <MainLayout />,
        children: [
          { index: true, element: <Navigate to="/turnos" replace /> },
          { path: '/turnos', element: <TurnosPage /> },
          {
            element: <RequireRole roles={['Admin']} />,
            children: [
              { path: '/pacientes', element: <PacientesPage /> },
              { path: '/profesionales', element: <ProfesionalesPage /> },
            ],
          },
          { path: '/403', element: <ForbiddenPage /> },
          { path: '*', element: <NotFoundPage /> },
        ],
      },
    ],
  },
])
