import { env } from '../config/env'
import { parseProblemDetails, type ParsedProblem } from './problemDetails'

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

  let res = await doFetch(anonymous ? null : getToken())

  if (res.status === 401 && !anonymous) {
    const newToken = await runRefresh()
    if (newToken) {
      res = await doFetch(newToken)
    } else {
      onAuthLost()
    }
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
