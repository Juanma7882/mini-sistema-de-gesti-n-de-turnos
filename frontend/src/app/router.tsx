import { lazy, Suspense } from 'react'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { RedirectIfAuthenticated } from '../core/auth/RedirectIfAuthenticated'
import { RequireAuth } from '../core/auth/RequireAuth'
import { RequireRole } from '../core/auth/RequireRole'
import { MainLayout } from '../core/layouts/MainLayout'
import { LoginPage } from '../features/auth/pages/LoginPage'
import { TurnosPage } from '../features/turnos/pages/TurnosPage'
import { RouteFallback } from '../shared/components/RouteFallback'

// Login y Turnos quedan estáticas (entrada + home de todo usuario logueado);
// el resto se carga diferido para no sumar al bundle inicial de un Profesional
// (openspec/changes/lazy-load-frontend-routes). Este archivo es config de
// rutas, no un módulo de componentes: fast refresh no aplica acá.
/* eslint-disable react-refresh/only-export-components */
const PacientesPage = lazy(() =>
  import('../features/pacientes/pages/PacientesPage').then((m) => ({ default: m.PacientesPage })),
)
const ProfesionalesPage = lazy(() =>
  import('../features/profesionales/pages/ProfesionalesPage').then((m) => ({
    default: m.ProfesionalesPage,
  })),
)
const ForbiddenPage = lazy(() =>
  import('../pages/ForbiddenPage').then((m) => ({ default: m.ForbiddenPage })),
)
const NotFoundPage = lazy(() =>
  import('../pages/NotFoundPage').then((m) => ({ default: m.NotFoundPage })),
)
/* eslint-enable react-refresh/only-export-components */

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
              {
                path: '/pacientes',
                element: (
                  <Suspense fallback={<RouteFallback />}>
                    <PacientesPage />
                  </Suspense>
                ),
              },
              {
                path: '/profesionales',
                element: (
                  <Suspense fallback={<RouteFallback />}>
                    <ProfesionalesPage />
                  </Suspense>
                ),
              },
            ],
          },
          {
            path: '/403',
            element: (
              <Suspense fallback={<RouteFallback />}>
                <ForbiddenPage />
              </Suspense>
            ),
          },
          {
            path: '*',
            element: (
              <Suspense fallback={<RouteFallback />}>
                <NotFoundPage />
              </Suspense>
            ),
          },
        ],
      },
    ],
  },
])
