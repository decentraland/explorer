import { Realm } from '../types'

export function realmToString(realm: Realm) {
  return realm.layer ? `${realm.catalystName}-${realm.layer}` : realm.catalystName
}
