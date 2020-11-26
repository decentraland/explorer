import { ContentMapping } from '../../types'

export type AssetId = string

export type Asset = {
  id: AssetId
  model: string
  mappings: ContentMapping[]
  baseUrl: string
}

const BASE_DOWNLOAD_URL = 'https://content.decentraland.org/contents'
const BASE_ASSET_URL = 'https://builder-api.decentraland.org/v1/assets'

export class AssetManager {
  private readonly assets: Map<AssetId, Asset> = new Map()

  async getAsset(assetId: AssetId): Promise<Asset | undefined> {
    const cachedAsset = this.assets.get(assetId)
    if (cachedAsset) {
      return cachedAsset
    }
    try {
      const response = await fetch(`${BASE_ASSET_URL}/${assetId}`)
      const { data }: { data: WebAsset } = await response.json()
      return this.webAssetToLocalAsset(data)
    } catch (e) {}
  }

  async getAssets(assetIds: AssetId[]): Promise<Map<AssetId, Asset>> {
    const knownAssets = assetIds.filter((assetId) => this.assets.has(assetId))
    const unknownAssets = assetIds.filter((assetId) => !this.assets.has(assetId))
    // TODO: If there are too many assets, we might end up over the url limit, so we might need to send multiple requests
    const queryParams = 'id=' + unknownAssets.join('&id=')
    try {
      const result: Map<AssetId, Asset> = new Map()
      // Fetch unknown assets
      const response = await fetch(`${BASE_ASSET_URL}?${queryParams}`)
      const { data }: { data: WebAsset[] } = await response.json()
      data.map((webAsset) => this.webAssetToLocalAsset(webAsset)).forEach((asset) => result.set(asset.id, asset))

      // Add known assets to result
      knownAssets.map((assetId) => this.assets.get(assetId)!).forEach((asset) => result.set(asset.id, asset))
    } catch (e) {}
    return new Map()
  }

  private webAssetToLocalAsset(webAsset: WebAsset): Asset {
    return {
      id: webAsset.id,
      model: webAsset.model,
      mappings: Object.entries(webAsset.contents).map(([file, hash]) => ({ file, hash })),
      baseUrl: BASE_DOWNLOAD_URL
    }
  }
}

type WebAsset = {
  id: string
  model: string
  contents: Record<string, string>
}
