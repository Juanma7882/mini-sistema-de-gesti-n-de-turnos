import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useLocation, useNavigate } from 'react-router-dom'
import { AlertCircle, Eye, EyeOff, HeartPulse, Loader2, Lock, Mail } from 'lucide-react'
import { useAuth } from '../../../core/auth/useAuth'
import { ApiError } from '../../../core/api/httpClient'
import { env } from '../../../core/config/env'
import { FieldError } from '../../../shared/components/FieldError'
import { useProblemForm } from '../../../shared/hooks/useProblemForm'
import { loginSchema, type LoginFormValues } from '../schemas/loginSchema'

interface LocationState {
  from?: { pathname: string }
}

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [formError, setFormError] = useState<string | null>(null)
  const [showPassword, setShowPassword] = useState(false)

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    // Ingreso rápido en dev: viene de VITE_DEMO_EMAIL/VITE_DEMO_PASSWORD (.env local,
    // no versionado); sin esas variables el form arranca vacío.
    defaultValues: env.demoCredentials,
  })

  const applyProblem = useProblemForm<LoginFormValues>(setError)

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null)
    try {
      await login(values.email, values.password)
      const to = (location.state as LocationState | null)?.from?.pathname ?? '/turnos'
      navigate(to, { replace: true })
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        setFormError('Email o contraseña incorrectos.')
        return
      }
      const detail = applyProblem(err)
      if (detail) setFormError(detail)
    }
  })

  return (
    <div className="grid min-h-dvh place-items-center bg-canvas px-5">
      <form
        onSubmit={onSubmit}
        noValidate
        className="w-full max-w-95 space-y-5 rounded-[10px] border border-border bg-surface p-6 sm:p-8"
      >
        <div className="flex flex-col items-center gap-3 text-center">
          <span className="flex size-11 items-center justify-center rounded-[10px] bg-primary-tint text-primary">
            <HeartPulse size={22} aria-hidden="true" />
          </span>
          <h1 className="text-login-title font-semibold">Ingresá a tu cuenta</h1>
        </div>

        {formError && (
          <p
            role="alert"
            className="flex items-center gap-2 rounded-[10px] bg-primary-tint px-3 py-2 text-sm text-destructive"
          >
            <AlertCircle size={16} className="shrink-0" aria-hidden="true" />
            {formError}
          </p>
        )}

        <div>
          <label className="text-sm font-medium" htmlFor="email">
            Email
          </label>
          <div className="relative mt-1">
            <Mail
              size={16}
              className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
              aria-hidden="true"
            />
            <input
              id="email"
              type="email"
              autoComplete="username"
              required
              placeholder="vos@ejemplo.com"
              aria-invalid={errors.email ? 'true' : 'false'}
              className="w-full rounded-[10px] border border-border py-2 pl-9 pr-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring"
              {...register('email')}
            />
          </div>
          <FieldError message={errors.email?.message} />
        </div>

        <div>
          <label className="text-sm font-medium" htmlFor="password">
            Contraseña
          </label>
          <div className="relative mt-1">
            <Lock
              size={16}
              className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
              aria-hidden="true"
            />
            <input
              id="password"
              type={showPassword ? 'text' : 'password'}
              autoComplete="current-password"
              required
              minLength={1}
              aria-invalid={errors.password ? 'true' : 'false'}
              className="w-full rounded-[10px] border border-border py-2 pr-10 pl-9 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring"
              {...register('password')}
            />
            <button
              type="button"
              onClick={() => setShowPassword((v) => !v)}
              aria-label={showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
              aria-pressed={showPassword}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
            >
              {showPassword ? (
                <EyeOff size={16} aria-hidden="true" />
              ) : (
                <Eye size={16} aria-hidden="true" />
              )}
            </button>
          </div>
          <FieldError message={errors.password?.message} />
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="flex w-full items-center justify-center gap-2 rounded-lg bg-primary px-3 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
        >
          {isSubmitting && <Loader2 size={16} className="animate-spin" aria-hidden="true" />}
          {isSubmitting ? 'Ingresando' : 'Ingresar'}
        </button>
      </form>
    </div>
  )
}
