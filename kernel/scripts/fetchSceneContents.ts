/**
 * Usage: Specify SCENE_ID or POINTER as evironment variables, and then run make fetchSceneContents in the kernel folder
 *
 * Example: POINTER='0,0' make fetchSceneContents
 *
 * The scene will be downloaded from https://peer.decentraland.org/content. This can be overriden with the CONTENT_SERVER_URL variable
 * The scene will be downloaded in the public/downloaded-scenes folder. This can be overriden with the OUTPUT_DIR variable
 */

import fetch from 'node-fetch'
import * as fs from 'fs'

const sceneId = process.env.SCENE_ID
const pointer = process.env.POINTER
const contentServerUrl = process.env.CONTENT_SERVER_URL ?? 'https://peer.decentraland.org/content'
const outputRoot = process.env.OUTPUT_DIR ?? 'public/downloaded-scenes'

const maxDownloads = parseInt(process.env.MAX_DOWNLOADS ?? '50')

if (!sceneId && !pointer) {
  console.log(
    'Please specify the scene id or pointer for which to download its contents using the SCENE_ID or POINTER env variables respectively'
  )
  process.exit(1)
}

function restrictLength(pending: string[]) {
  if (pending.length > 5) {
    return `${pending.length}`
  } else {
    return JSON.stringify(pending)
  }
}

type Content = {
  file: string
  hash: string
}

async function main() {
  const query = sceneId ? `id=${sceneId}` : `pointer=${pointer}`
  const url = `${contentServerUrl}/entities/scene?${query}`
  const sceneDataResponse = await fetch(url)
  if (sceneDataResponse.ok) {
    const pending: Record<string, Promise<any>> = {}
    const queued: Content[] = []
    const sceneData = (await sceneDataResponse.json())[0]

    const scenePath = `${outputRoot}/${sceneData.pointers[0]}-scene-${sceneData.id}`

    await fs.promises.mkdir(scenePath, { recursive: true })

    function download(content: Content) {
      pending[content.file] = fetch(url)
        .then(async (response) => {
          if (response.ok) {
            const pathParts = (content.file as string).split('/')
            if (pathParts.length > 0) {
              pathParts.pop()
              const folder = pathParts.join('/')
              await fs.promises.mkdir(`${scenePath}/${folder}`, { recursive: true })
            }
            const fileStream = fs.createWriteStream(`${scenePath}/${content.file}`)

            await new Promise((resolve, reject) => {
              response.body.pipe(fileStream)
              response.body.on('error', (e) => {
                delete pending[content.file]
                console.log(
                  `Error downloading file ${content.file}. Pending: ${restrictLength(
                    Object.keys(pending)
                  )}. Queued: ${restrictLength(queued.map((it) => it.file))}`,
                  e
                )
                reject(e)
              })
              fileStream.on('finish', () => {
                delete pending[content.file]
                console.log(
                  `Finished downloading file ${content.file}. Pending: ${restrictLength(
                    Object.keys(pending)
                  )}  Queued: ${restrictLength(queued.map((it) => it.file))}`
                )
                resolve()
              })
            }).finally(() => {
              if (queued.length > 0) {
                download(queued.shift())
              }
            })
          } else {
            const text = await sceneDataResponse.text()
            console.log(
              `Unexpected response from server downloading file ${content.file}. Status: ${sceneDataResponse.status}; Body: ${text}`
            )
          }
        })
        .catch((e) => {
          // Ignored. This is handled inside
        })
    }

    for (let content of sceneData.content) {
      const url = `${contentServerUrl}/contents/${content.hash}`
      console.log(`Downloading ${content.file} from ${url}`)
      if (Object.keys(pending).length < maxDownloads) {
        download(content)
      } else {
        queued.push(content)
      }
    }

    while (Object.values(pending).length > 0) {
      await Promise.all(Object.values(pending))
    }
    
  } else {
    const text = await sceneDataResponse.text()
    throw new Error(`Unexpected response from server. Status: ${sceneDataResponse.status}; Body: ${text}`)
  }
}

main().then(
  () => console.log('Process finished successfully'),
  (e) => {
    console.log('Process failed', e)
    process.exit(1)
  }
)
