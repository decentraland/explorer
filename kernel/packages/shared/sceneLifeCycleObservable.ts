import { Observable } from '../decentraland-ecs/src/ecs/Observable'

export const sceneLifeCycleObservable = new Observable<{ sceneId: string; status: string }>()
