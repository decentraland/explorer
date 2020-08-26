declare var dcl: any

let modulePromise: any

type PositionType = { x: number, y: number, z: number }

/**
 * move to inside scene position
 *
 * @param position PositionType
 * @param cameraTarget PositionType
 */
export function moveTo(position: PositionType, cameraTarget?: PositionType) {
  callModuleRpc('requestMoveTo', [position, cameraTarget])
}

function ensureModule(): boolean {
  if (typeof modulePromise === 'undefined' && typeof dcl !== 'undefined') {
    modulePromise = dcl.loadModule('@decentraland/RestrictedActionModule')
  }
  return typeof modulePromise !== 'undefined' && typeof dcl !== 'undefined'
}

function callModuleRpc(methodName: string, args: any[]): void {
  if (ensureModule()) {
    modulePromise.then(($: any) => {
      dcl.callRpc($.rpcHandle, methodName, args)
    })
  }
}
