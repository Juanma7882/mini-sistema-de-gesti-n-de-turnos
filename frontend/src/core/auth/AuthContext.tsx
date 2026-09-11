import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { configureHttpClient } from '../api/httpClient'
import { authApi, type AuthUser } from './authApi'
import { authBroadcast } from './authBroadcast'
import { session } from './session'

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous'

export interface AuthContextValue {
  status: AuthStatus
  user: AuthUser | null
  login: (email: string, password: string) => Promise<AuthUser>
  logout: () => Promise<void>
}

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>('loading')
  const [user, setUser] = useState<AuthUser | null>(null)

  const clearSession = useCallback(() => {
    session.setToken(null)
    session.writeSnapshot(null)
    setUser(null)
    setStatus('anonymous')
  }, [])

  const applySession = useCallback((token: string, nextUser: AuthUser) => {
    session.setToken(token)
    session.writeSnapshot({ nombre: nextUser.nombre, role: nextUser.role })
    setUser(nextUser)
    setStatus('authenticated')
  }, [])

  // Un 401 en cualquier request dispara esto (vía httpClient); también corre al bootear.
  const refresh = useCallback(async (): Promise<string | null> => {
    try {
      const { token } = await authApi.refresh()
      session.setToken(token)
      const me = await authApi.me()
      applySession(token, me)
      return token
    } catch {
      clearSession()
      return null
    }
  }, [applySession, clearSession])

  // Inyecta los hooks de sesión en el cliente HTTP (interceptor 401).
  useEffect(() => {
    configureHttpClient({ getToken: session.getToken, refresh, onAuthLost: clearSession })
  }, [refresh, clearSession])

  // Hidratación optimista al montar: sin flash de /login (arquitectura-frontend.md §5.3).
  useEffect(() => {
    void refresh()
  }, [refresh])

  // Sincronización entre pestañas.
  useEffect(
    () =>
      authBroadcast.subscribe((event) => {
        if (event === 'logout') clearSession()
        if (event === 'login') void refresh()
      }),
    [clearSession, refresh],
  )

  const login = useCallback(
    async (email: string, password: string) => {
      const { token, user: nextUser } = await authApi.login(email, password)
      applySession(token, nextUser)
      authBroadcast.publish('login')
      return nextUser
    },
    [applySession],
  )

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } finally {
      clearSession()
      authBroadcast.publish('logout')
    }
  }, [clearSession])

  const value = useMemo<AuthContextValue>(
    () => ({ status, user, login, logout }),
    [status, user, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
