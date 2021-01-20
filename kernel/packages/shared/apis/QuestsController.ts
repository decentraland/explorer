import { ExposableAPI } from './ExposableAPI'
import { exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { ClientResponse, PlayerQuestDetails, ProgressData } from 'dcl-quests-client'
import { questsRequest } from '../quests/client'

export type QuestsResponse<T> = Omit<ClientResponse<T>, 'status'>

@registerAPI('QuestsController')
export class QuestsController extends ExposableAPI {
  @exposeMethod
  async getQuests(): Promise<QuestsResponse<PlayerQuestDetails[]>> {
    return questsRequest((client) => client.getQuests())
  }

  @exposeMethod
  async getQuestDetails(questId: string): Promise<QuestsResponse<PlayerQuestDetails>> {
    return questsRequest((client) => client.getQuestDetails(questId))
  }

  @exposeMethod
  async startQuest(questId: string): Promise<QuestsResponse<PlayerQuestDetails>> {
    return questsRequest((client) => client.startQuest(questId))
  }

  @exposeMethod
  async makeProgress(
    questId: string,
    taskId: string,
    progress: ProgressData
  ): Promise<QuestsResponse<PlayerQuestDetails>> {
    return questsRequest((client) => client.makeProgress(questId, taskId, progress))
  }
}
