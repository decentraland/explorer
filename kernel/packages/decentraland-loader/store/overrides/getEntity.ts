import { Mapping } from '../sceneInfo/types'
export function getEntity(id: string, pointers: string[], metadata: any, content: Mapping[]) {
  return {
    id,
    type: 'scene',
    pointers,
    metadata,
    content
  }
}
