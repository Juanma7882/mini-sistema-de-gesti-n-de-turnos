import { Component, type ErrorInfo, type ReactNode } from 'react'
import { Toaster } from 'sonner'
import { AlertCircle, AlertOctagon, CheckCircle2 } from 'lucide-react'
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
        <div className="grid min-h-dvh place-items-center bg-canvas text-center">
          <div className="flex flex-col items-center gap-3">
            <span className="grid size-11 place-items-center rounded-[10px] bg-primary-tint text-destructive">
              <AlertOctagon size={22} aria-hidden="true" />
            </span>
            <p className="text-sm text-muted-foreground">Algo se rompió al mostrar esta pantalla.</p>
            <button
              onClick={() => window.location.reload()}
              className="rounded-lg bg-primary px-3 py-2 text-sm font-medium text-primary-foreground"
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
        <Toaster
          position="top-right"
          icons={{
            success: <CheckCircle2 size={18} aria-hidden="true" />,
            error: <AlertCircle size={18} aria-hidden="true" />,
          }}
          toastOptions={{
            classNames: {
              toast: 'rounded-[10px] border font-sans',
              success: '!border-[#c7e4d0] !bg-[#DFEEE4] !text-[#3E7A55]',
              error: '!border-primary-tint !bg-primary-tint !text-destructive',
            },
          }}
        />
      </AuthProvider>
    </ErrorBoundary>
  )
}
