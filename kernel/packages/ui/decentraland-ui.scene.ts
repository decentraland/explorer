import { executeTask, DecentralandInterface } from 'decentraland-ecs'
import { avatarMessageObservable } from './avatar/avatarSystem'

declare const dcl: DecentralandInterface

// Initialize avatar profile scene

executeTask(async () => {
  await Promise.all([dcl.loadModule('@decentraland/Identity'), dcl.loadModule('@decentraland/SocialController')])

  dcl.subscribe('AVATAR_OBSERVABLE')

  dcl.onEvent((event) => {
    const eventType: string = event.type

    if (eventType === 'AVATAR_OBSERVABLE') {
      avatarMessageObservable.notifyObservers(event.data as any)
    }
  })
})
