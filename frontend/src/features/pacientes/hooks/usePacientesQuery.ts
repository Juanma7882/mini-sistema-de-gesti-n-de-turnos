import { useCallback } from 'react'
import { usePagedResource } from '../../../shared/hooks/usePagedResource'
import { pacientesApi, type PacientesQuery } from '../api/pacientesApi'

export function usePacientesQuery(query: PacientesQuery) {
  const fetcher = useCallback(
    () => pacientesApi.listar(query),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [query.search, query.page, query.pageSize],
  )
  return usePagedResource(fetcher)
}
