const rawApiUrl = import.meta.env.VITE_API_URL as string | undefined

if (!rawApiUrl && import.meta.env.PROD) {
  throw new Error('VITE_API_URL no está definida en el build de producción.')
}

const demoEmail = import.meta.env.VITE_DEMO_EMAIL as string | undefined
const demoPassword = import.meta.env.VITE_DEMO_PASSWORD as string | undefined

export const env = {
  /** URL base de la API, sin barra final. */
  apiUrl: (rawApiUrl ?? 'http://localhost:5000/api').replace(/\/+$/, ''),
  /** Prellenado del login en dev (.env local, no versionado); ausente si no se configuró. */
  demoCredentials:
    import.meta.env.DEV && demoEmail && demoPassword
      ? { email: demoEmail, password: demoPassword }
      : undefined,
} as const
