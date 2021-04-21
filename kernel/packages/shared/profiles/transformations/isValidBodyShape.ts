const BODY_SHAPES = [
  'urn:decentraland:off-chain:base-avatars:BaseFemale',
  'urn:decentraland:off-chain:base-avatars:BaseMale'
]

export function isValidBodyShape(shape: string) {
  return BODY_SHAPES.includes(shape)
}
