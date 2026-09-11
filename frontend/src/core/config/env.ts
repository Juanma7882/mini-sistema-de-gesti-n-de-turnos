const rawApiUrl = import.meta.env.VITE_API_URL as string | undefined

if (!rawApiUrl && import.meta.env.PROD) {
  throw new Error('VITE_API_URL no está definida en el build de producción.')
}

export const env = {
  /** URL base de la API, sin barra final. */
  apiUrl: (rawApiUrl ?? 'http://localhost:5000/api').replace(/\/+$/, ''),
} as const
