using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestsNotificationsHUDView : MonoBehaviour
    {
        private const float SECTION_NOTIFICATION_DURATION = 1.5f;

        private readonly Queue<GameObject> notificationsQueue = new Queue<GameObject>();

        [SerializeField] private GameObject sectionCompletedPrefab;
        [SerializeField] private GameObject sectionUnlockedPrefab;
        [SerializeField] private GameObject questCompletedPrefab;

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
        }

        public void ShowSectionCompleted(QuestSection section)
        {
            var questNotification = Instantiate(sectionCompletedPrefab, transform).GetComponent<QuestNotification_SectionCompleted>();
            questNotification.Populate(section);
            questNotification.gameObject.SetActive(false);
            notificationsQueue.Enqueue(questNotification.gameObject);
        }

        public void ShowSectionUnlocked(QuestSection section)
        {
            var questNotification = Instantiate(sectionUnlockedPrefab, transform).GetComponent<QuestNotification_SectionUnlocked>();
            questNotification.Populate(section);
            questNotification.gameObject.SetActive(false);
            notificationsQueue.Enqueue(questNotification.gameObject);
        }

        public void ShowQuestCompleted(QuestModel quest)
        {
            var questNotification = Instantiate(questCompletedPrefab, transform).GetComponent<QuestNotification_QuestCompleted>();
            questNotification.Populate(quest);
            questNotification.gameObject.SetActive(false);
            notificationsQueue.Enqueue(questNotification.gameObject);
        }

        private IEnumerator ProcessSectionsNotificationQueue()
        {
            while (true)
            {
                if (notificationsQueue.Count > 0)
                {
                    GameObject notificationGO = notificationsQueue.Dequeue();
                    notificationGO.gameObject.SetActive(true);
                    yield return WaitForSecondsCache.Get(SECTION_NOTIFICATION_DURATION);
                    Destroy(notificationGO);
                }

                yield return WaitForSecondsCache.Get(0.5f);
            }
        }
    }
}
