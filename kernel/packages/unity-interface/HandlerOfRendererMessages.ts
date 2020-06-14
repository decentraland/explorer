import { browserInterfaceType } from 'shared/renderer-interface/browserInterface/browserInterfaceType'

export type HandlerOfRendererMessages = (type: keyof browserInterfaceType, message: any) => void
