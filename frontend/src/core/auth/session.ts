export type Role = 'Admin' | 'Profesional'

const SNAPSHOT_KEY = 'mae-turnos:user'

/** Snapshot NO sensible, solo para pintar el menú al primer render. */
export interface UserSnapshot {
  nombre: string
  role: Role
}

// El access token vive SOLO en memoria (arquitectura-frontend.md §5.1).
let accessToken: string | null = null

export const session = {
  getToken: (): string | null => accessToken,
  setToken: (token: string | null): void => {
    accessToken = token
  },

  readSnapshot(): UserSnapshot | null {
    try {
      const raw = localStorage.getItem(SNAPSHOT_KEY)
      return raw ? (JSON.parse(raw) as UserSnapshot) : null
    } catch {
      return null
    }
  },

  writeSnapshot(snapshot: UserSnapshot | null): void {
    try {
      if (snapshot) localStorage.setItem(SNAPSHOT_KEY, JSON.stringify(snapshot))
      else localStorage.removeItem(SNAPSHOT_KEY)
    } catch {
      // almacenamiento no disponible: no es crítico
    }
  },
}
