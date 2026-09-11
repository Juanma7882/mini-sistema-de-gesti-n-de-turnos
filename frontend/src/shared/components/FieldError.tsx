import { AlertCircle } from 'lucide-react'

export function FieldError({ message }: { message?: string }) {
  if (!message) return null
  return (
    <p className="mt-1 flex items-center gap-1 text-xs text-destructive">
      <AlertCircle size={14} aria-hidden="true" />
      {message}
    </p>
  )
}
