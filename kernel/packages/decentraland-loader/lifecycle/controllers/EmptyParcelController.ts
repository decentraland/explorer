import { jsonFetch } from 'atomicHelpers/jsonFetch'
import { ILand, ContentMapping } from 'shared/types'

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
