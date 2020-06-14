import { globalDCL } from 'shared/globalDCL';
import { defaultLogger } from 'shared/logger';

export async function fetchOwner(name: string) {
  const query = `
    query GetOwner($name: String!) {
      nfts(first: 1, where: { searchText: $name }) {
        owner{
          address
        }
      }
    }`;

  const variables = { name: name.toLowerCase() };

  try {
    const resp = await queryGraph(query, variables);
    return resp.data.nfts.length === 1 ? (resp.data.nfts[0].owner.address as string) : null;
  }
  catch (error) {
    defaultLogger.error(`Error querying graph`, error);
    throw error;
  }
}

async function queryGraph(query: string, variables: any) {
  const url = 'https://api.thegraph.com/subgraphs/name/decentraland/marketplace';
  const opts = {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ query, variables })
  };
  const res = await fetch(url, opts);
  return res.json();
}

export function toSocialId(userId: string) {
  const domain = globalDCL.globalStore.getState().chat.privateMessaging.client?.getDomain();
  return `@${userId.toLowerCase()}:${domain}`;
}
