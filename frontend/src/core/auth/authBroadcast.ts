type AuthEvent = 'login' | 'logout'

const channel: BroadcastChannel | null =
  typeof BroadcastChannel !== 'undefined' ? new BroadcastChannel('mae-turnos:auth') : null

/** Sincroniza login/logout entre pestañas (arquitectura-frontend.md §5.4). */
export const authBroadcast = {
  publish(event: AuthEvent): void {
    channel?.postMessage(event)
  },
  subscribe(handler: (event: AuthEvent) => void): () => void {
    if (!channel) return () => {}
    const listener = (e: MessageEvent<AuthEvent>) => handler(e.data)
    channel.addEventListener('message', listener)
    return () => channel.removeEventListener('message', listener)
  },
}
