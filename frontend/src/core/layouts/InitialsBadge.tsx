export function InitialsBadge({ nombre }: { nombre: string }) {
  const initials = nombre
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('')

  return (
    <span className="grid size-9 shrink-0 place-items-center rounded-full bg-primary-tint text-sm font-semibold text-primary-strong">
      {initials || '?'}
    </span>
  )
}
