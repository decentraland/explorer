using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestsNotificationsHUDView : MonoBehaviour
    {
        private const float SECTION_NOTIFICATION_DURATION = 1.5f;

        private enum NotificationType
        {
            Completed,
            Unlocked
        }

        private readonly Queue<(QuestSection section, NotificationType notificationType)> notificationsQueue = new Queue<(QuestSection, NotificationType)>();

        [SerializeField] private SectionNotification_Completed completedNotification;
        [SerializeField] private SectionNotification_Unlocked unlockedNotification;

        internal static QuestsNotificationsHUDView Create()
        {
            QuestsNotificationsHUDView view = Instantiate(Resources.Load<GameObject>("QuestsNotificationsHUD")).GetComponent<QuestsNotificationsHUDView>();
#if UNITY_EDITOR
            view.gameObject.name = "_QuestsNotificationsHUDView";
#endif
            return view;
        }

        private void Awake()
        {
            StartCoroutine(ProcessSectionsNotificationQueue());
            completedNotification.gameObject.SetActive(false);
            unlockedNotification.gameObject.SetActive(false);
        }

        public void ShowSectionCompleted(QuestSection section)
        {
            notificationsQueue.Enqueue((section, NotificationType.Completed));
        }

        public void ShowSectionUnlocked(QuestSection section)
        {
            notificationsQueue.Enqueue((section, NotificationType.Unlocked));
        }

        private IEnumerator ProcessSectionsNotificationQueue()
        {
            while (true)
            {
                if (notificationsQueue.Count > 0)
                {
                    var notification = notificationsQueue.Dequeue();
                    if (notification.notificationType == NotificationType.Completed)
                        completedNotification.Populate(notification.section);
                    else
                        unlockedNotification.Populate(notification.section);
                    completedNotification.gameObject.SetActive(notification.notificationType == NotificationType.Completed);
                    unlockedNotification.gameObject.SetActive(notification.notificationType == NotificationType.Unlocked);
                    yield return WaitForSecondsCache.Get(SECTION_NOTIFICATION_DURATION);
                    completedNotification.gameObject.SetActive(false);
                    unlockedNotification.gameObject.SetActive(false);
                }

                yield return WaitForSecondsCache.Get(0.5f);
            }
        }
    }
}
