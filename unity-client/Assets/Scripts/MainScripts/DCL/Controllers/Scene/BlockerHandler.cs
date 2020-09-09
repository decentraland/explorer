using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers
{
    public class BlockerHandler
    {
        static GameObject blockerPrefab;

        const string PARCEL_BLOCKER_POOL_NAME = "ParcelBlocker";
        const string PARCEL_BLOCKER_PREFAB = "Prefabs/ParcelBlocker";

        protected Vector3 auxPosVec = new Vector3();
        protected Vector3 auxScaleVec = new Vector3();
        protected Dictionary<Vector2Int, PoolableObject> blockers = new Dictionary<Vector2Int, PoolableObject>();

        protected static Vector2Int[] aroundOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1)
        };

        public BlockerHandler()
        {
            if (blockerPrefab == null)
                blockerPrefab = Resources.Load<GameObject>(PARCEL_BLOCKER_PREFAB);

            // We need to manually create the Pool for empty game objects if it doesn't exist
            if (!PoolManager.i.ContainsPool(PARCEL_BLOCKER_POOL_NAME))
            {
                GameObject go = Object.Instantiate(blockerPrefab);
                Pool pool = PoolManager.i.AddPool(PARCEL_BLOCKER_POOL_NAME, go);
                pool.persistent = true;
                pool.ForcePrewarm();
            }
        }

        public void SetupSceneBlockers(HashSet<Vector2Int> parcels, float height, Transform parent)
        {
            CleanBlockers();

            auxScaleVec.x = ParcelSettings.PARCEL_SIZE;
            auxScaleVec.y = height;
            auxScaleVec.z = ParcelSettings.PARCEL_SIZE;

            auxPosVec.y = (height - 1) / 2;

            using (var it = parcels.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    Vector2Int pos = it.Current;

                    bool isSurrounded = true;

                    for (int i = 0; i < aroundOffsets.Length; i++)
                    {
                        Vector2Int o = aroundOffsets[i];

                        if (!parcels.Contains(new Vector2Int(pos.x + o.x, pos.y + o.y)))
                        {
                            isSurrounded = false;
                            break;
                        }
                    }

                    if (isSurrounded)
                        continue;

                    InstantiateBlocker(pos, parent);
                }
            }
        }

        protected void InstantiateBlocker(Vector2Int pos, Transform parent)
        {
            float centerOffset = ParcelSettings.PARCEL_SIZE / 2;
            PoolableObject blockerPoolable = PoolManager.i.Get(PARCEL_BLOCKER_POOL_NAME);
            Transform blockerTransform = blockerPoolable.gameObject.transform;

            blockerTransform.SetParent(parent, false);
            blockerTransform.position = DCLCharacterController.i.characterPosition.WorldToUnityPosition(Utils.GridToWorldPosition(pos.x, pos.y));

            auxPosVec.x = blockerTransform.position.x + centerOffset;
            auxPosVec.z = blockerTransform.position.z + centerOffset;

            blockerTransform.position = auxPosVec;
            blockerTransform.localScale = auxScaleVec;

#if UNITY_EDITOR
            blockerPoolable.gameObject.name = "BLOCKER " + pos;
#endif

            blockers.Add(pos, blockerPoolable);
        }

        public void CleanBlockers()
        {
            foreach (var blocker in blockers)
            {
                blocker.Value.Release();
            }

            blockers.Clear();
        }
    }
}