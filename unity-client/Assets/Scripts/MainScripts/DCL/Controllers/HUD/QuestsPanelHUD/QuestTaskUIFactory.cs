using System;
using System.Linq;
using UnityEngine;

namespace DCL.Huds.QuestPanel
{
    [CreateAssetMenu(menuName = "Variables/QuestTaskFactory", fileName = "QuestTaskFactory", order = 0)]
    public class QuestTaskUIFactory : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string type;
            public GameObject prefab;
        }

        [SerializeField] private Entry[] entries;

        public GameObject GetPrefab(string type)
        {
            return entries.FirstOrDefault(x => x.type == type)?.prefab;
        }
    }
}