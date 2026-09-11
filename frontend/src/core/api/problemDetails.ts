import type { ProblemDetails } from '../../shared/types/api'

export interface ParsedProblem {
  title: string
  detail: string
  /** errores por campo (presente solo en 400 de validación). */
  errors: Record<string, string[]>
}

/**
 * Único parser de ProblemDetails de la app (arquitectura-frontend.md §4).
 * Devuelve una forma normalizada aunque el body no sea un ProblemDetails válido.
 */
export async function parseProblemDetails(res: Response): Promise<ParsedProblem> {
  let body: ProblemDetails = {}
  try {
    body = (await res.clone().json()) as ProblemDetails
  } catch {
    // respuesta sin cuerpo JSON: se usan los defaults de abajo
  }
  return {
    title: body.title ?? `Error ${res.status}`,
    detail: body.detail ?? res.statusText ?? 'Ocurrió un error inesperado.',
    errors: body.errors ?? {},
  }
}
