export function realmToString(realm: ({ catalystName: string } | { serverName: string }) & { layer?: string }) {
  const name = "catalystName" in realm ? realm.catalystName : realm.serverName

  return realm.layer ? `${name}-${realm.layer}` : name
}
