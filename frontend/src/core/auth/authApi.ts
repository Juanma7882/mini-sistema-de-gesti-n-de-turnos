import { httpClient } from '../api/httpClient'
import type { Role } from './session'

export type { Role }

export interface AuthUser {
  id: string
  nombre: string
  email: string
  role: Role
  profesionalId?: string
}

export interface LoginResponse {
  token: string
  user: AuthUser
}

/** Endpoints de /auth (README.md → Auth). `login` y `refresh` van sin Bearer. */
export const authApi = {
  login: (email: string, password: string) =>
    httpClient.post<LoginResponse>('/auth/login', { email, password }, { anonymous: true }),
  refresh: () =>
    httpClient.post<{ token: string }>('/auth/refresh', undefined, { anonymous: true }),
  logout: () => httpClient.post<void>('/auth/logout'),
  // skipAuthRetry: se llama desde dentro de `refresh()`; un 401 acá no debe reintentar refresh (evita deadlock sobre refreshInFlight).
  me: () => httpClient.get<AuthUser>('/auth/me', { skipAuthRetry: true }),
}
