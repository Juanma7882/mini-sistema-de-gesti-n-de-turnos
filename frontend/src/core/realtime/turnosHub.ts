import * as signalR from '@microsoft/signalr'
import { env } from '../config/env'
import { session } from '../auth/session'
import type { TurnoDto } from '../../features/turnos/types'

const EVENTO_TURNO_CAMBIADO = 'turnoCambiado'

// El hub vive en la raíz de la API (no bajo /api), a diferencia de httpClient.
const hubUrl = (): string => `${new URL(env.apiUrl).origin}/hubs/turnos`

let connection: signalR.HubConnection | null = null
const handlers = new Set<(turno: TurnoDto) => void>()

/** Push de cambios de turnos entre sesiones, vía SignalR (arquitectura-backend.md). */
export const turnosHub = {
  connect(): void {
    if (connection) return
    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl(), { accessTokenFactory: () => session.getToken() ?? '' })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()
    connection.on(EVENTO_TURNO_CAMBIADO, (turno: TurnoDto) => {
      handlers.forEach((handler) => handler(turno))
    })
    connection.start().catch(() => {
      // Sin conexión en tiempo real no hay funcionalidad crítica que se rompa:
      // el usuario sigue viendo datos frescos al navegar/recargar.
    })
  },

  disconnect(): void {
    const current = connection
    connection = null
    void current?.stop()
  },

  subscribe(handler: (turno: TurnoDto) => void): () => void {
    handlers.add(handler)
    return () => handlers.delete(handler)
  },
}
