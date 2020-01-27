using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers
{
    public class BlockerHandler
    {
        private readonly Dictionary<(int, int), PoolableObject> blockers = new Dictionary<(int, int), PoolableObject>();

        private static GameObject blockerPrefab;
        const string PARCEL_BLOCKER_POOL_NAME = "ParcelBlocker";
        private const string PARCEL_BLOCKER_PREFAB = "Prefabs/ParcelBlocker";

        Vector3 auxPosVec = new Vector3();
        Vector3 auxScaleVec = new Vector3();

        private static Vector2Int[] aroundOffsets = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1) };

        public BlockerHandler()
        {
            if (blockerPrefab == null)
                blockerPrefab = Resources.Load<GameObject>(PARCEL_BLOCKER_PREFAB);

            // We need to manually create the Pool for empty game objects if it doesn't exist
            if (!PoolManager.i.ContainsPool(PARCEL_BLOCKER_POOL_NAME))
            {
                GameObject go = Object.Instantiate(blockerPrefab);
                Pool pool = PoolManager.i.AddPool(PARCEL_BLOCKER_POOL_NAME, go);
                pool.ForcePrewarm();
            }
        }


        public void SetupBlockers(Vector2Int[] parcels, float height, Transform parent)
        {
            CleanBlockers();

            int parcelsLength = parcels.Length;

            auxScaleVec.x = ParcelSettings.PARCEL_SIZE;
            auxScaleVec.y = height;
            auxScaleVec.z = ParcelSettings.PARCEL_SIZE;

            Vector3 parcelCenter = new Vector3(ParcelSettings.PARCEL_SIZE / 2, 0, ParcelSettings.PARCEL_SIZE / 2);

            auxPosVec.y = (height / 2) + blockerPrefab.transform.localPosition.y;

            for (int i = 0; i < parcelsLength; i++)
            {
                Vector2Int pos = parcels[i];

                bool isSurrounded = true;

                for (int i1 = 0; i1 < aroundOffsets.Length; i1++)
                {
                    Vector2Int o = aroundOffsets[i1];

                    if (!blockers.ContainsKey((pos.x + o.x, pos.y + o.y)))
                    {
                        isSurrounded = false;
                        break;
                    }
                }

                if (isSurrounded)
                    continue;

                PoolableObject blocker = PoolManager.i.Get(PARCEL_BLOCKER_POOL_NAME);

                Transform blockerTransform = blocker.gameObject.transform;
                blockerTransform.SetParent(parent, false);

                Vector3 tmpPos = DCLCharacterController.i.characterPosition.WorldToUnityPosition(Utils.GridToWorldPosition(pos.x, pos.y)) + parcelCenter;
                auxPosVec.x = tmpPos.x;
                auxPosVec.z = tmpPos.z;

                blockerTransform.position = auxPosVec;
                blockerTransform.localScale = auxScaleVec;

                blockers.Add((pos.x, pos.y), blocker);
            }
        }

        public void CleanBlockers()
        {
            using (var it = blockers.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    it.Current.Value.Release();
                }
            }

            blockers.Clear();
        }
    }
}
