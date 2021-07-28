import * as crypto from 'crypto'

export function generateNonceForChallenge(challenge: string, complexity: number): string {
  while (true) {
    const nonce = crypto.randomBytes(256).toString('hex')
    const hash = crypto
      .createHash('sha256')
      .update(challenge + nonce)
      .digest('hex')
    const isValid = hash.startsWith('0'.repeat(complexity))

    if (isValid) {
      return nonce
    }
  }
}
