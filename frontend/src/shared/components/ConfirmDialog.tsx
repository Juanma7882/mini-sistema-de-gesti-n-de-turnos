interface ConfirmDialogProps {
  open: boolean
  title: string
  description?: string
  confirmLabel?: string
  onConfirm: () => void
  onCancel: () => void
}

// TODO(frontend.md §1.3): reemplazar por shadcn/ui <AlertDialog>.
export function ConfirmDialog({
  open,
  title,
  description,
  confirmLabel = 'Confirmar',
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  if (!open) return null
  return (
    <div
      className="fixed inset-0 z-50 grid place-items-center bg-black/40"
      role="dialog"
      aria-modal="true"
    >
      <div className="w-full max-w-sm rounded-lg bg-background p-6 shadow-lg">
        <h2 className="text-base font-semibold">{title}</h2>
        {description && <p className="mt-2 text-sm text-muted-foreground">{description}</p>}
        <div className="mt-6 flex justify-end gap-2">
          <button onClick={onCancel} className="rounded-md border px-3 py-1.5 text-sm">
            Cancelar
          </button>
          <button
            onClick={onConfirm}
            className="rounded-md bg-primary px-3 py-1.5 text-sm text-primary-foreground"
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}
