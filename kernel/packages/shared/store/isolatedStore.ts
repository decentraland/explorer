import { Store } from 'redux'
import { RootState } from './rootTypes'

export let store: Store<RootState>

export function setStore(_store: Store<RootState>) {
  store = _store
}
