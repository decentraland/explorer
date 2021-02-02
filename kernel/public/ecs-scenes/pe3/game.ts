import { spawn, kill, PortableExperienceHandle } from '@decentraland/PortableExperiences'

executeTask(async () => {
  try {
    const peId: string = 'urn:decentraland:off-chain:static-portable-experiences:pe1'
    const portableExperienceHandle: PortableExperienceHandle = await spawn({ urn: peId })

    sleep(30000)

    await kill(portableExperienceHandle.pid)
  } catch {
    log('Error starting/stopping portable experience')
  }
})

function sleep(milliseconds) {
  const date = Date.now()
  let currentDate = null
  do {
    currentDate = Date.now()
  } while (currentDate - date < milliseconds)
}
