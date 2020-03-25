import { expect } from 'chai'
import sinon from 'sinon'
import { jsonFetch } from 'atomicHelpers/jsonFetch'

describe('jsonFetch', function() {
  it('does not cache a result if it fails', async () => {
    const fake = sinon.stub()
    fake.onCall(0).returns(Promise.resolve({ ok: false }))
    fake.returns(Promise.resolve({ ok: true, json: () => ({ stub: true }) }))
    globalThis.fetch = window.fetch = fake

    try {
      await jsonFetch('fakeurl')
      expect.fail('first jsonFetch should reject')
    } catch (e) {}

    await jsonFetch('fakeurl')

    sinon.assert.calledTwice(fake)
  })

  it('caches a result if it succeeds', async () => {
    const fake = sinon.stub()
    fake.returns(Promise.resolve({ ok: true, json: () => ({ stub: true }) }))
    globalThis.fetch = window.fetch = fake

    const r1 = await jsonFetch('anotherfakeurl')
    const r2 = await jsonFetch('anotherfakeurl')

    sinon.assert.calledOnce(fake)
    expect(r1).to.eq(r2)
  })
})
