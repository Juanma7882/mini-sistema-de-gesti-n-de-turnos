import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../../../core/auth/useAuth'
import { FieldError } from '../../../shared/components/FieldError'
import { loginSchema, type LoginFormValues } from '../schemas/loginSchema'

interface LocationState {
  from?: { pathname: string }
}

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [formError, setFormError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) })

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null)
    try {
      await login(values.email, values.password)
      const to = (location.state as LocationState | null)?.from?.pathname ?? '/turnos'
      navigate(to, { replace: true })
    } catch {
      setFormError('Credenciales inválidas.')
    }
  })

  return (
    <div className="grid min-h-dvh place-items-center">
      <form onSubmit={onSubmit} className="w-full max-w-sm space-y-4 rounded-lg border p-6">
        <h1 className="text-lg font-semibold">Ingresar</h1>

        <div>
          <label className="text-sm" htmlFor="email">
            Email
          </label>
          <input
            id="email"
            type="email"
            autoComplete="username"
            className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
            {...register('email')}
          />
          <FieldError message={errors.email?.message} />
        </div>

        <div>
          <label className="text-sm" htmlFor="password">
            Contraseña
          </label>
          <input
            id="password"
            type="password"
            autoComplete="current-password"
            className="mt-1 w-full rounded-md border px-3 py-2 text-sm"
            {...register('password')}
          />
          <FieldError message={errors.password?.message} />
        </div>

        {formError && <p className="text-sm text-destructive">{formError}</p>}

        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full rounded-md bg-primary px-3 py-2 text-sm text-primary-foreground disabled:opacity-50"
        >
          {isSubmitting ? 'Ingresando…' : 'Ingresar'}
        </button>
      </form>
    </div>
  )
}
