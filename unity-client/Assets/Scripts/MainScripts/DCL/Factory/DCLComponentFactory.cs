using System;
using DCL.Components;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class DCLComponentFactory : ScriptableObject
    {
        [System.Serializable]
        public class Item
        {
            public CLASS_ID_COMPONENT classId;
            public BaseComponent prefab;

            [Header("Pool Options")] public bool usePool;
            public int prewarmCount;

            [NonSerialized] public Pool pool;
        }

        public Item[] factoryList;

        Dictionary<CLASS_ID_COMPONENT, Item> factoryDict;

        public void EnsureFactoryDictionary()
        {
            if (factoryDict == null)
            {
                factoryDict = new Dictionary<CLASS_ID_COMPONENT, Item>();

                for (int i = 0; i < factoryList.Length; i++)
                {
                    Item item = factoryList[i];

                    if (!factoryDict.ContainsKey(item.classId))
                    {
                        factoryDict.Add(item.classId, item);
                    }
                }
            }
        }

        public CLASS_ID_COMPONENT GetIdForType<T>() where T : Component
        {
            for (int i = 0; i < factoryList.Length; i++)
            {
                Item item = factoryList[i];

                if (item != null && item.prefab != null && item.prefab.GetComponent<T>() != null)
                {
                    return item.classId;
                }
            }

            return CLASS_ID_COMPONENT.NONE;
        }

        public void PrewarmPools()
        {
            for (int i = 0; i < factoryList.Length; i++)
            {
                Item item = factoryList[i];

                if (item.usePool && item.pool == null)
                {
                    GameObject original = Instantiate(item.prefab.gameObject);
                    item.pool = PoolManager.i.AddPool(item.classId.ToString() + "_POOL", original, maxPrewarmCount: item.prewarmCount, isPersistent: true);
                    item.pool.ForcePrewarm();
                }
            }
        }

        public ItemType CreateItemFromId<ItemType>(CLASS_ID_COMPONENT id)
            where ItemType : BaseComponent
        {
            EnsureFactoryDictionary();

            if (!factoryDict.ContainsKey(id))
            {
#if UNITY_EDITOR
                Debug.LogError("Class " + id + " can't be instantiated because the field doesn't exist!");
#endif
                return default(ItemType);
            }

            var factoryItem = factoryDict[id];

            if (factoryItem.prefab == null)
            {
                Debug.LogError("Prefab for class " + id + " is null!");
                return default(ItemType);
            }

            GameObject instancedGo;
            PoolableObject poolableObject = null;

            if (factoryItem.usePool)
            {
                poolableObject = factoryItem.pool.Get();
                instancedGo = poolableObject.gameObject;
            }
            else
            {
                instancedGo = Instantiate(factoryItem.prefab.gameObject);
            }

            ItemType item = instancedGo.GetComponent<ItemType>();
            item.poolableObject = poolableObject;

            return item;
        }
    }
}