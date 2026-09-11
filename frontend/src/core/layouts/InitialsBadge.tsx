export function InitialsBadge({ nombre }: { nombre: string }) {
  const initials = nombre
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('')

  return (
    <span className="grid size-8 place-items-center rounded-full bg-primary/10 text-xs font-medium text-primary">
      {initials || '?'}
    </span>
  )
}
