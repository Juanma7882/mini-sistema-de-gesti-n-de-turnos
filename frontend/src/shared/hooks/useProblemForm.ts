import { useCallback } from 'react'
import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import { ApiError } from '../../core/api/httpClient'

/**
 * Vuelca los `errors` por campo de un ProblemDetails (400) a react-hook-form
 * (arquitectura-frontend.md §4). Devuelve el `detail` general cuando el error no
 * era de validación por campo, o `null` si ya se repartió a los campos.
 */
export function useProblemForm<T extends FieldValues>(setError: UseFormSetError<T>) {
  return useCallback(
    (err: unknown): string | null => {
      if (!(err instanceof ApiError)) return 'Ocurrió un error inesperado.'

      const fieldErrors = err.problem.errors
      const keys = Object.keys(fieldErrors)

      if (err.status === 400 && keys.length > 0) {
        for (const key of keys) {
          const field = (key.charAt(0).toLowerCase() + key.slice(1)) as Path<T>
          setError(field, { type: 'server', message: fieldErrors[key]?.join(' ') })
        }
        return null
      }
      return err.problem.detail
    },
    [setError],
  )
}
