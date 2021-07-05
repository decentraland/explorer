import { KernelConfigForRenderer } from 'shared/types'
import { commConfigurations, getDefaultTLD, WSS_ENABLED } from 'config'
import { nameValidCharacterRegex, nameValidRegex } from 'shared/profiles/utils/names'
import { getWorld } from '@dcl/schemas'
import { isFeatureEnabled } from "shared/meta/selectors"
import { FeatureFlags } from "shared/meta/types"
import { StoreContainer } from "shared/store/rootTypes"

declare const globalThis: { UnityLoader: any } & StoreContainer

export function kernelConfigForRenderer(): KernelConfigForRenderer {
  return {
    comms: {
      commRadius: commConfigurations.commRadius,
      voiceChatEnabled: false
    },
    profiles: {
      nameValidCharacterRegex: nameValidCharacterRegex.toString().replace(/[/]/g, ''),
      nameValidRegex: nameValidRegex.toString().replace(/[/]/g, '')
    },
    features: {
      enableBuilderInWorld: false,
      enableAvatarLODs: isFeatureEnabled(globalThis.globalStore.getState(), FeatureFlags.AVATAR_LODS, false)
    },
    gifSupported:
      // tslint:disable-next-line
      typeof OffscreenCanvas !== 'undefined' && typeof OffscreenCanvasRenderingContext2D === 'function' && !WSS_ENABLED,
    tld: getDefaultTLD(),
    validWorldRanges: getWorld().validWorldRanges
  }
}
