import { describe, expect, it } from 'vitest'
import { transicionesPermitidas } from './machine'

describe('transicionesPermitidas', () => {
  it('desde Pendiente ofrece Confirmado y Cancelado', () => {
    expect(transicionesPermitidas('Pendiente', 'Admin')).toEqual(['Confirmado', 'Cancelado'])
  })

  it('desde Confirmado ofrece Atendido y Cancelado', () => {
    expect(transicionesPermitidas('Confirmado', 'Profesional')).toEqual(['Atendido', 'Cancelado'])
  })

  it('los estados terminales no ofrecen transiciones', () => {
    expect(transicionesPermitidas('Cancelado', 'Admin')).toEqual([])
    expect(transicionesPermitidas('Atendido', 'Admin')).toEqual([])
  })
})
