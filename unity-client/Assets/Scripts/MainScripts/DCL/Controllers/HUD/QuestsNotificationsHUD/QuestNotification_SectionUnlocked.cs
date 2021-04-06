using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestNotification_SectionUnlocked : MonoBehaviour, IQuestNotification
    {
        [SerializeField] internal TextMeshProUGUI sectionName;
        [SerializeField] internal TextMeshProUGUI taskName;

        public void Populate(QuestSection section)
        {
            sectionName.text = section.name;
            taskName.text = section.tasks.FirstOrDefault()?.name;
        }

        public void Show() { gameObject.SetActive(true); }

        public void Dispose() { Destroy(gameObject); }
        public IEnumerator Waiter() { yield return WaitForSecondsCache.Get(QuestsNotificationsHUDView.DEFAULT_NOTIFICATION_DURATION); }
    }
}