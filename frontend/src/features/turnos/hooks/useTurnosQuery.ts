import { useCallback } from 'react'
import { usePagedResource } from '../../../shared/hooks/usePagedResource'
import { turnosApi, type TurnosQuery } from '../api/turnosApi'

export function useTurnosQuery(query: TurnosQuery) {
  const fetcher = useCallback(
    () => turnosApi.listar(query),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [
      query.desde,
      query.hasta,
      query.estado,
      query.pacienteId,
      query.profesionalId,
      query.page,
      query.pageSize,
    ],
  )
  return usePagedResource(fetcher)
}
