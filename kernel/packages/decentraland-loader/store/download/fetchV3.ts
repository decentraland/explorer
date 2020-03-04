import { SceneMetadata, SceneContentServerEntity } from '../sceneInfo/types'
import { ConfigState } from '../config/config'

export async function fetchScenesFromServer(coordinates: string[], config: ConfigState): Promise<SceneMetadata[]> {
  const catalyst = config.contentServer
  const data = await fetch(catalyst + '/entities/scene?pointer=' + coordinates.join('&pointer='))
  const scenes = await data.json()
  return scenes.map((scene: SceneContentServerEntity) => ({
    ...scene,
    metadata: {
      sceneId: scene.id,
      baseUrl: catalyst + '/contents/',
      baseUrlBundles: config.contentServerBundles + '/',
      name: (scene.metadata as any).scene.title,
      scene: scene.metadata,
      mappingsResponse: {
        parcel_id: scene.id,
        publisher: scene.id,
        root_cid: scene.id,
        contents: scene.content
      }
    }
  }))
}
