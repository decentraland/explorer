using DCL;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public interface IQuestsController
{
    event Action<string> OnQuestProgressed;
}

public class QuestsController : MonoBehaviour, IQuestsController
{
    public event Action<string> OnQuestProgressed;

    #region just for testing, dont merge this code. (IF you see this in a review, hit me)
    [SerializeField] private TextAsset questsJson;
    private void Start()
    {
        var quests = Utils.ParseJsonArray<List<QuestModel>>(questsJson.text);
        for (var i = quests.Count - 1; i >= 0; i--)
        {
            DataStore.Quests.quests.Add(quests[i].id, quests[i]);
        }
    }
    [ContextMenu("Add Entry")]
    private void UtilAddEntry()
    {
        var quests = Utils.ParseJsonArray<List<QuestModel>>(questsJson.text);
        OnQuestProgressed?.Invoke(quests[Random.Range(0, quests.Count)].id);
    }
    #endregion

}
