using System.Collections;
using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestNotification_SectionCompleted : MonoBehaviour, IQuestNotification
    {
        [SerializeField] internal TextMeshProUGUI sectionName;

        public void Populate(QuestSection section) { sectionName.text = section.name; }

        public void Show() { gameObject.SetActive(true); }

        public void Dispose() { Destroy(gameObject); }
        public IEnumerator Waiter() { yield return WaitForSecondsCache.Get(QuestsNotificationsHUDView.DEFAULT_NOTIFICATION_DURATION); }
    }
}