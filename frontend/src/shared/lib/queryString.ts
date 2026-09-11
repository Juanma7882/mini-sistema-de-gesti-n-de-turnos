type ParamValue = string | number | boolean | undefined | null

/** Arma URLSearchParams descartando vacíos/undefined (para sincronizar filtros ↔ URL). */
export function toSearchParams(record: Record<string, ParamValue>): URLSearchParams {
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(record)) {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, String(value))
    }
  }
  return params
}

export function readParam(params: URLSearchParams, key: string): string | undefined {
  return params.get(key) ?? undefined
}
