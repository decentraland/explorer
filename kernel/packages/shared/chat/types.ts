import { SocialAPI } from 'dcl-social-client'

export type ChatState = {
  privateMessaging: {
    client: SocialAPI | null
    socialInfo: Record<string, SocialData>
    friends: string[]
    toFriendRequests: string[]
    fromFriendRequests: string[]
  }
}

export type RootChatState = {
  chat: ChatState
}

export type SocialData = {
  userId: string
  socialId: string
  conversationId?: string
}
