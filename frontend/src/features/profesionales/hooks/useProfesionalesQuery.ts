import { useCallback } from 'react'
import { usePagedResource } from '../../../shared/hooks/usePagedResource'
import { profesionalesApi, type ProfesionalesQuery } from '../api/profesionalesApi'

export function useProfesionalesQuery(query: ProfesionalesQuery) {
  const fetcher = useCallback(
    () => profesionalesApi.listar(query),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [query.search, query.page, query.pageSize],
  )
  return usePagedResource(fetcher)
}
