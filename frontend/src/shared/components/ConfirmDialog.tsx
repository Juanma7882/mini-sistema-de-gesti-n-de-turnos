import { AlertTriangle, type LucideIcon } from 'lucide-react'
import { Dialog } from './Dialog'

interface ConfirmDialogProps {
  open: boolean
  title: string
  description?: string
  confirmLabel?: string
  icon?: LucideIcon
  destructive?: boolean
  onConfirm: () => void
  onCancel: () => void
}

export function ConfirmDialog({
  open,
  title,
  description,
  confirmLabel = 'Confirmar',
  icon: Icon = AlertTriangle,
  destructive = true,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  return (
    <Dialog open={open} title={title} onClose={onCancel}>
      <div className="flex flex-col items-start gap-3">
        <span className="grid size-11 place-items-center rounded-full bg-primary-tint text-destructive">
          <Icon size={20} aria-hidden="true" />
        </span>
        {description && <p className="text-sm text-muted-foreground">{description}</p>}
      </div>
      <div className="mt-6 flex justify-end gap-2">
        <button
          type="button"
          onClick={onCancel}
          className="rounded-lg border border-border px-3 py-1.5 text-sm text-foreground hover:bg-primary-tint"
        >
          Cancelar
        </button>
        <button
          type="button"
          onClick={onConfirm}
          className={`rounded-lg px-3 py-1.5 text-sm font-medium text-primary-foreground ${
            destructive ? 'bg-destructive' : 'bg-primary'
          }`}
        >
          {confirmLabel}
        </button>
      </div>
    </Dialog>
  )
}
