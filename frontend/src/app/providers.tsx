import { Component, type ErrorInfo, type ReactNode } from 'react'
import { Toaster } from 'sonner'
import { AuthProvider } from '../core/auth/AuthContext'

interface ErrorBoundaryState {
  hasError: boolean
}

/** Boundary raíz para errores de render (arquitectura-frontend.md §4). */
class ErrorBoundary extends Component<{ children: ReactNode }, ErrorBoundaryState> {
  state: ErrorBoundaryState = { hasError: false }

  static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('ErrorBoundary', error, info)
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="grid min-h-dvh place-items-center text-center">
          <div>
            <p className="text-lg font-semibold">Algo salió mal.</p>
            <button
              onClick={() => window.location.reload()}
              className="mt-4 rounded-md border px-3 py-1.5 text-sm"
            >
              Recargar
            </button>
          </div>
        </div>
      )
    }
    return this.props.children
  }
}

export function Providers({ children }: { children: ReactNode }) {
  return (
    <ErrorBoundary>
      <AuthProvider>
        {children}
        <Toaster richColors position="top-right" />
      </AuthProvider>
    </ErrorBoundary>
  )
}
