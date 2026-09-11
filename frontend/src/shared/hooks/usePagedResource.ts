import { useCallback, useEffect, useState } from 'react'
import { ApiError } from '../../core/api/httpClient'
import type { PagedResult } from '../types/api'

interface State<T> {
  data: PagedResult<T> | null
  isLoading: boolean
  error: ApiError | null
}

function emptyPage<T>(): PagedResult<T> {
  return { items: [], total: 0, page: 1, pageSize: 20 }
}

/**
 * Estado genérico de una lista paginada del servidor
 * (arquitectura-frontend.md §5.5). `fetcher` debe ser estable: envolverlo en
 * `useCallback` dentro del hook de la feature.
 */
export function usePagedResource<T>(fetcher: () => Promise<PagedResult<T>>) {
  const [state, setState] = useState<State<T>>({ data: null, isLoading: true, error: null })

  const load = useCallback(async () => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }))
    try {
      const data = await fetcher()
      setState({ data, isLoading: false, error: null })
    } catch (err) {
      setState({ data: null, isLoading: false, error: err instanceof ApiError ? err : null })
    }
  }, [fetcher])

  useEffect(() => {
    void load()
  }, [load])

  return {
    data: state.data ?? emptyPage<T>(),
    isLoading: state.isLoading,
    error: state.error,
    refetch: load,
  }
}
