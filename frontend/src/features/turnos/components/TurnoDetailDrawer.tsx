import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Phone, SearchX, Shield, Stethoscope, X, XCircle } from 'lucide-react'
import { toast } from 'sonner'
import { useAuth } from '../../../core/auth/useAuth'
import { ApiError } from '../../../core/api/httpClient'
import { ConfirmDialog } from '../../../shared/components/ConfirmDialog'
import { formatInicio } from '../../../shared/lib/formatInicio'
import type { EstadoTurno } from '../../../shared/types/api'
import { turnosApi } from '../api/turnosApi'
import type { TurnoDto } from '../types'
import { EstadoBadge } from './EstadoBadge'
import { EstadoControl } from './EstadoControl'

/** Refleja `?turno=<id>` en la URL: recargable, cerrable con Escape o click afuera. */
export function TurnoDetailDrawer({ onEdit }: { onEdit: (turno: TurnoDto) => void }) {
  const [searchParams, setSearchParams] = useSearchParams()
  const turnoId = searchParams.get('turno')
  const { user } = useAuth()

  const [turno, setTurno] = useState<TurnoDto | null>(null)
  const [notFound, setNotFound] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [estadoError, setEstadoError] = useState<string | null>(null)
  const [confirmCancel, setConfirmCancel] = useState(false)

  const close = () => {
    const next = new URLSearchParams(searchParams)
    next.delete('turno')
    setSearchParams(next, { replace: true })
  }

  useEffect(() => {
    if (!turnoId) {
      setTurno(null)
      setNotFound(false)
      return
    }
    setIsLoading(true)
    setNotFound(false)
    setEstadoError(null)
    turnosApi
      .obtener(turnoId)
      .then(setTurno)
      .catch((err) => {
        if (err instanceof ApiError && err.status === 404) setNotFound(true)
      })
      .finally(() => setIsLoading(false))
  }, [turnoId])

  useEffect(() => {
    if (!turnoId) return
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') close()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [turnoId])

  const onCambiarEstado = async (siguiente: EstadoTurno) => {
    if (!turno) return
    setEstadoError(null)
    try {
      const updated = await turnosApi.cambiarEstado(turno.id, siguiente)
      setTurno(updated)
      toast.success(siguiente === 'Cancelado' ? 'Turno cancelado' : 'Estado actualizado')
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setEstadoError(err.problem.detail)
      }
    }
  }

  const onSolicitarCambio = (siguiente: EstadoTurno) => {
    if (siguiente === 'Cancelado') {
      setConfirmCancel(true)
      return
    }
    void onCambiarEstado(siguiente)
  }

  if (!turnoId) return null

  return (
    <div className="fixed inset-0 z-50">
      <button
        type="button"
        aria-label="Cerrar"
        onClick={close}
        className="absolute inset-0 bg-foreground/30"
      />
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Detalle del turno"
        className="absolute right-0 top-0 flex h-dvh w-full max-w-105 flex-col gap-5 overflow-y-auto bg-surface p-5 shadow-[0_0_24px_-4px_rgba(162,60,99,0.35)]"
      >
        <div className="flex items-center justify-between">
          <h2 className="text-dialog-title font-semibold text-foreground">Detalle del turno</h2>
          <button
            type="button"
            onClick={close}
            aria-label="Cerrar"
            className="rounded-lg p-1.5 text-muted-foreground hover:bg-primary-tint hover:text-primary-strong"
          >
            <X size={18} aria-hidden="true" />
          </button>
        </div>

        {isLoading && <p className="text-sm text-muted-foreground">Cargando…</p>}

        {notFound && (
          <div className="flex flex-col items-center gap-3 py-10 text-center">
            <SearchX size={28} className="text-muted-foreground" aria-hidden="true" />
            <p className="text-sm text-muted-foreground">Este turno ya no está disponible.</p>
            <button
              type="button"
              onClick={close}
              className="text-sm font-medium text-primary-strong underline"
            >
              Volver al listado
            </button>
          </div>
        )}

        {turno && !isLoading && (
          <>
            <section className="space-y-2 border-b border-border pb-4">
              <div className="flex items-center justify-between">
                <p className="text-sm font-medium text-foreground">{formatInicio(turno.inicio)}</p>
                <EstadoBadge estado={turno.estado} />
              </div>
              {turno.notas && <p className="text-sm text-muted-foreground">{turno.notas}</p>}

              {estadoError && (
                <p
                  role="alert"
                  className="rounded-[10px] bg-primary-tint px-3 py-2 text-sm text-destructive"
                >
                  {estadoError}
                </p>
              )}
              {user && (
                <EstadoControl estado={turno.estado} role={user.role} onCambiar={onSolicitarCambio} />
              )}
            </section>

            <section className="space-y-1.5 border-b border-border pb-4">
              <p className="text-meta font-medium text-muted-foreground">Paciente</p>
              <p className="text-sm text-foreground">
                {turno.paciente.nombre} {turno.paciente.apellido}
              </p>
              <a
                href={`tel:${turno.paciente.telefono}`}
                className="flex items-center gap-1.5 text-sm text-primary-strong"
              >
                <Phone size={14} aria-hidden="true" />
                {turno.paciente.telefono}
              </a>
              <p className="flex items-center gap-1.5 text-sm text-muted-foreground">
                <Shield size={14} aria-hidden="true" />
                {turno.paciente.obraSocial}
              </p>
            </section>

            <section className="space-y-1.5">
              <p className="text-meta font-medium text-muted-foreground">Profesional</p>
              <p className="flex items-center gap-1.5 text-sm text-foreground">
                <Stethoscope size={14} aria-hidden="true" />
                {turno.profesional.nombre} {turno.profesional.apellido} · {turno.profesional.especialidad}
              </p>
            </section>

            {user?.role === 'Admin' && (
              <button
                type="button"
                onClick={() => onEdit(turno)}
                className="mt-auto rounded-lg border border-border px-3 py-1.5 text-sm text-foreground hover:bg-primary-tint"
              >
                Editar turno
              </button>
            )}
          </>
        )}
      </div>

      <ConfirmDialog
        open={confirmCancel}
        title="Cancelar turno"
        description="El horario quedará libre para otro paciente."
        confirmLabel="Sí, cancelar"
        icon={XCircle}
        onConfirm={() => {
          setConfirmCancel(false)
          void onCambiarEstado('Cancelado')
        }}
        onCancel={() => setConfirmCancel(false)}
      />
    </div>
  )
}
