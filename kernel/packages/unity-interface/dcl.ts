import { globalDCL } from 'shared/globalDCL'
import { browserInterface } from './browserInterface'
import { unityInterface } from './unityInterface'

const rendererVersion = require('decentraland-renderer')
window['console'].log('Renderer version: ' + rendererVersion)

globalDCL.browserInterface = browserInterface

export const CHUNK_SIZE = 100

globalDCL.unityInterface = unityInterface
globalDCL.rendererInterface = unityInterface
globalDCL.builderInterface = unityInterface
