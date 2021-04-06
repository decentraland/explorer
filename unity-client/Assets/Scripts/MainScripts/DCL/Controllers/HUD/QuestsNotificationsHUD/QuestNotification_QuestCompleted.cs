using System.Collections;
using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestNotification_QuestCompleted : MonoBehaviour, IQuestNotification
    {
        [SerializeField] internal TextMeshProUGUI questName;

        public void Populate(QuestModel questModel) { questName.text = questModel.name; }

        public void Show() { gameObject.SetActive(true); }

        public void Dispose() { Destroy(gameObject); }
        public IEnumerator Waiter() { yield return WaitForSecondsCache.Get(QuestsNotificationsHUDView.DEFAULT_NOTIFICATION_DURATION); }
    }
}