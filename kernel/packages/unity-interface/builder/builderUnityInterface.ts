import { ReadOnlyVector3 } from 'decentraland-ecs'
import { unityInterfaceType } from '../unityInterface/unityInterfaceType'

export type builderUnityInterface = unityInterfaceType & {
  SelectGizmoBuilder: (type: string) => void
  ResetBuilderObject: () => void
  SetCameraZoomDeltaBuilder: (delta: number) => void
  GetCameraTargetBuilder: (futureId: string) => void
  SetPlayModeBuilder: (on: string) => void
  PreloadFileBuilder: (url: string) => void
  GetMousePositionBuilder: (x: string, y: string, id: string) => void
  TakeScreenshotBuilder: (id: string) => void
  SetCameraPositionBuilder: (position: ReadOnlyVector3) => void
  SetCameraRotationBuilder: (aplha: number, beta: number) => void
  ResetCameraZoomBuilder: () => void
  SetBuilderGridResolution: (position: number, rotation: number, scale: number) => void
  SetBuilderSelectedEntities: (entities: string[]) => void
  ResetBuilderScene: () => void
  OnBuilderKeyDown: (key: string) => void
}
