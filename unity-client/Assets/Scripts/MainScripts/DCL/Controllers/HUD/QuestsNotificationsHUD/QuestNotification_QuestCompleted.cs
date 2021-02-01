using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestNotification_QuestCompleted : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questName;

        public void Populate(QuestModel questModel)
        {
            questName.text = questModel.name;
            //TODO Rewards definition is needed here
        }
    }
}