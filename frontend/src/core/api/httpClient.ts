import { createElement } from 'react'
import { toast } from 'sonner'
import { WifiOff } from 'lucide-react'
import { env } from '../config/env'
import { parseProblemDetails, type ParsedProblem } from './problemDetails'

const CONNECTION_ERROR_MESSAGE = 'No pudimos conectar con el servidor. Probá de nuevo en un momento.'

/** Toast único para 500 / error de red, sin exponer detalle técnico (frontend-app-shell spec). */
function notifyConnectionError(): void {
  toast.error(CONNECTION_ERROR_MESSAGE, {
    id: 'connection-error',
    icon: createElement(WifiOff, { size: 18, 'aria-hidden': true }),
  })
}

/** Error tipado con el que rechaza toda llamada fallida; nunca se propaga un `Response` crudo. */
export class ApiError extends Error {
  readonly status: number
  readonly problem: ParsedProblem

  constructor(status: number, problem: ParsedProblem) {
    super(problem.detail)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

type TokenGetter = () => string | null
type RefreshFn = () => Promise<string | null>
type OnAuthLost = () => void

// Hooks de sesión, inyectados por AuthProvider (arquitectura-frontend.md §5).
let getToken: TokenGetter = () => null
let refresh: RefreshFn = async () => null
let onAuthLost: OnAuthLost = () => {}

export function configureHttpClient(opts: {
  getToken: TokenGetter
  refresh: RefreshFn
  onAuthLost: OnAuthLost
}): void {
  getToken = opts.getToken
  refresh = opts.refresh
  onAuthLost = opts.onAuthLost
}

// Un único POST /auth/refresh en vuelo; el resto de requests 401 espera este.
let refreshInFlight: Promise<string | null> | null = null

function runRefresh(): Promise<string | null> {
  refreshInFlight ??= refresh().finally(() => {
    refreshInFlight = null
  })
  return refreshInFlight
}

export interface RequestOptions extends Omit<RequestInit, 'body'> {
  body?: unknown
  /** No adjunta Authorization ni intenta refresh (login / refresh). */
  anonymous?: boolean
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { anonymous = false, body, headers, ...rest } = options

  const doFetch = (token: string | null): Promise<Response> =>
    fetch(`${env.apiUrl}${path}`, {
      ...rest,
      credentials: 'include',
      headers: {
        ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...headers,
      },
      body: body !== undefined ? JSON.stringify(body) : undefined,
    })

  let res: Response
  try {
    res = await doFetch(anonymous ? null : getToken())
  } catch {
    notifyConnectionError()
    throw new ApiError(0, { title: 'Network Error', detail: CONNECTION_ERROR_MESSAGE, errors: {} })
  }

  if (res.status === 401 && !anonymous) {
    const newToken = await runRefresh()
    if (newToken) {
      try {
        res = await doFetch(newToken)
      } catch {
        notifyConnectionError()
        throw new ApiError(0, { title: 'Network Error', detail: CONNECTION_ERROR_MESSAGE, errors: {} })
      }
    } else {
      onAuthLost()
    }
  }

  if (res.status >= 500) {
    notifyConnectionError()
  }

  if (!res.ok) {
    throw new ApiError(res.status, await parseProblemDetails(res))
  }
  if (res.status === 204) {
    return undefined as T
  }
  return (await res.json()) as T
}

export const httpClient = {
  get: <T>(path: string, options?: RequestOptions) =>
    request<T>(path, { ...options, method: 'GET' }),
  post: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { ...options, method: 'POST', body }),
  put: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { ...options, method: 'PUT', body }),
  patch: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { ...options, method: 'PATCH', body }),
  delete: <T>(path: string, options?: RequestOptions) =>
    request<T>(path, { ...options, method: 'DELETE' }),
}
