import { defaultLogger } from 'shared/logger'
import { ErrorContextTypes, ReportFatalErrorWithUnityPayloadAsync } from 'shared/loading/ReportFatalError'
import { unityInterface, UnityInterface } from './UnityInterface'

export class ClientDebug {
  private unityInterface: UnityInterface

  public constructor(unityInterface: UnityInterface) {
    this.unityInterface = unityInterface
  }

  public DumpScenesLoadInfo() {
    this.unityInterface.SendMessageToUnity('Main', 'DumpScenesLoadInfo')
  }

  public DumpRendererLockersInfo() {
    this.unityInterface.SendMessageToUnity('Main', 'DumpRendererLockersInfo')
  }

  public RunPerformanceMeterTool(durationInSeconds: number) {
    this.unityInterface.SendMessageToUnity('Main', 'RunPerformanceMeterTool', durationInSeconds)
  }

  public TestErrorReport(message: string, context: ErrorContextTypes) {
    ReportFatalErrorWithUnityPayloadAsync(new Error(message), context)
      .then(() => defaultLogger.log(`Report sent success.`))
      .catch(() => defaultLogger.log(`Report sent fail.`))

    defaultLogger.log(`Report being sent.`)
  }

  public DumpCrashPayload() {
    this.unityInterface
      .CrashPayloadRequest()
      .then((payload: string) => {
        defaultLogger.log(`DumpCrashPayload result:\n${payload}`)
        defaultLogger.log(`DumpCrashPayload length:${payload.length}`)
      })
      .catch((x) => {
        defaultLogger.log(`DumpCrashPayload result: timeout`)
      })
  }

  public InstantiateBotsAtWorldPos(amount: number, xPos: number, yPos: number, zPos: number, areaWidth: number, areaDepth: number) {
    const payload = {
      amount: amount,
      xPos: xPos,
      yPos: yPos,
      zPos: zPos,
      areaWidth: areaWidth,
      areaDepth: areaDepth
    }
    this.unityInterface.SendMessageToUnity('Main', 'InstantiateBotsAtWorldPos', JSON.stringify(payload))
  }

  public InstantiateBotsAtCoords(amount: number, xCoord: number, yCoord: number, areaWidth: number, areaDepth: number) {
    const payload = {
      amount: amount,
      xCoord: xCoord,
      yCoord: yCoord,
      areaWidth: areaWidth,
      areaDepth: areaDepth
    }
    this.unityInterface.SendMessageToUnity('Main', 'InstantiateBotsAtCoords', JSON.stringify(payload))
  }

  public RemoveBot(targetEntityId: string) {
    this.unityInterface.SendMessageToUnity('Main', 'RemoveBot', targetEntityId)
  }

  public ClearBots() {
    this.unityInterface.SendMessageToUnity('Main', 'ClearBots')
  }
}

export let clientDebug: ClientDebug = new ClientDebug(unityInterface)
