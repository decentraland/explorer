import { jsonFetch } from 'atomicHelpers/jsonFetch'
import { ILand, ContentMapping } from 'shared/types'
import { HALLOWEEN } from 'config'
import defaultLogger from 'shared/logger'

export class EmptyParcelController {
  emptyScenes!: Record<string, ContentMapping[]>
  emptyScenesPromise?: Promise<Record<string, ContentMapping[]>>
  emptySceneNames: string[] = []
  baseUrl: string = ''

  constructor(
    public options: {
      contentServer: string
      metaContentServer: string
      metaContentService: string
      contentServerBundles: string
    }
  ) {
    this.baseUrl = globalThis.location.origin + '/loader/empty-scenes-halloween/'
    // if (HALLOWEEN) {
    //   defaultLogger.info('USING HALLOWEEN EMPTY SCENES')
    // } else {
    //   defaultLogger.info('USING DEFAULT EMPTY SCENES... ' + location.origin)
    //   this.baseUrl = globalThis.location.origin + '/loader/empty-scenes/'
    // }
  }

  resolveEmptyParcels() {
    if (this.emptyScenesPromise) {
      return
    }

    this.emptyScenesPromise = jsonFetch(this.baseUrl + 'index.json').then((scenes) => {
      this.emptySceneNames = Object.keys(scenes)
      this.emptyScenes = scenes
      return this.emptyScenes
    })
  }

  isEmptyParcel(sceneId: string): boolean {
    return sceneId.endsWith('00000000000000000000')
  }

  createFakeILand(sceneId: string, coordinates: string): ILand {
    const sceneName = this.emptySceneNames[Math.floor(Math.random() * this.emptySceneNames.length)]

    return {
      sceneId: sceneId,
      baseUrl: this.baseUrl + 'contents/',
      baseUrlBundles: this.options.contentServerBundles,
      sceneJsonData: {
        display: { title: 'Empty parcel' },
        contact: { name: 'Decentraland' },
        owner: '',
        main: `bin/game.js`,
        tags: [],
        scene: { parcels: [coordinates], base: coordinates },
        policy: {},
        communications: { commServerUrl: '' }
      },
      mappingsResponse: {
        parcel_id: coordinates,
        root_cid: sceneId,
        contents: this.emptyScenes[sceneName]
      }
    }
  }
}
