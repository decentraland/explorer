using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerEntry : MonoBehaviour
    {
        private const float DELAY_TO_DESTROY = 0.5f;
        private static readonly int OUT_ANIM_TRIGGER = Animator.StringToHash("Out");
        public event Action OnLayoutRebuildRequested;

        [SerializeField] internal TextMeshProUGUI questTitle;
        [SerializeField] internal RawImage questIcon;
        [SerializeField] internal TextMeshProUGUI sectionTitle;
        [SerializeField] internal Image progress;
        [SerializeField] internal RectTransform tasksContainer;
        [SerializeField] internal GameObject taskPrefab;
        [SerializeField] internal Button expandCollapseButton;
        [SerializeField] internal GameObject expandIcon;
        [SerializeField] internal GameObject collapseIcon;
        [SerializeField] internal Toggle pinQuestToggle;
        [SerializeField] internal RawImage iconImage;
        [SerializeField] internal Animator containerAnimator;

        private AssetPromise_Texture iconPromise;
        private float progressTarget = 0;

        internal QuestModel quest;
        private bool isExpanded;
        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;

        private readonly Dictionary<string, QuestsTrackerTask> taskEntries = new Dictionary<string, QuestsTrackerTask>();

        public void Awake()
        {
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);

            expandCollapseButton.gameObject.SetActive(false);
            SetExpandCollapseState(true);
            expandCollapseButton.onClick.AddListener(() => SetExpandCollapseState(!isExpanded));
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;
            questTitle.text = quest.name;
            SetIcon(quest.thumbnail_entry);
            QuestSection currentSection = quest.sections.First(x => x.progress < 1f);
            sectionTitle.text = $"{currentSection.name} - {currentSection.tasks.Count(x => x.progress >= 1)}/{currentSection.tasks.Length}";
            progressTarget = currentSection.progress;

            //Initialize it with all the task and purge the list as we process the new ones.
            List<string> taskToRemove = taskEntries.Keys.ToList();

            for (int index = 0; index < currentSection.tasks.Length; index++)
            {
                QuestTask task = currentSection.tasks[index];
                taskToRemove.Remove(task.id);
                AddOrUpdateTask(task, index);
            }

            for (int index = 0; index < taskToRemove.Count; index++)
            {
                string taskId = taskToRemove[index];
                if (taskEntries.TryGetValue(taskId, out QuestsTrackerTask taskEntry))
                    Destroy(taskEntry.gameObject);
                taskEntries.Remove(taskId);
            }

            expandCollapseButton.gameObject.SetActive(taskEntries.Count > 0);
            SetExpandCollapseState(true);
            OnLayoutRebuildRequested?.Invoke();
        }

        internal void AddOrUpdateTask(QuestTask task, int siblingIndex)
        {
            if (taskEntries.TryGetValue(task.id, out QuestsTrackerTask taskEntry))
            {
                taskEntry.Populate(task);
                if (task.progress >= 1)
                {
                    //TODO Replace Destroy by Task Completed Anim
                    taskEntry.StartDestroy(() =>
                    {
                        taskEntries.Remove(task.id);
                        OnLayoutRebuildRequested?.Invoke();
                    });
                    return;
                }
            }
            else
            {
                if (task.progress >= 1)
                    return;

                taskEntry = Instantiate(taskPrefab, tasksContainer).GetComponent<QuestsTrackerTask>();
                taskEntries.Add(task.id, taskEntry);
                taskEntry.Populate(task);
            }
            taskEntry.transform.SetSiblingIndex(siblingIndex);
        }

        internal void SetIcon(string iconURL)
        {
            if (iconPromise != null)
            {
                iconPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(iconPromise);
            }

            if (string.IsNullOrEmpty(iconURL))
                return;

            iconPromise = new AssetPromise_Texture(iconURL);
            iconPromise.OnSuccessEvent += OnIconReady;
            iconPromise.OnFailEvent += x => { Debug.Log($"Error downloading quest tracker entry icon: {iconURL}"); };

            AssetPromiseKeeper_Texture.i.Keep(iconPromise);
        }

        private void OnIconReady(Asset_Texture assetTexture) { iconImage.texture = assetTexture.texture; }

        internal void SetExpandCollapseState(bool newIsExpanded)
        {
            isExpanded = newIsExpanded;
            expandIcon.SetActive(!isExpanded);
            collapseIcon.SetActive(isExpanded);
            tasksContainer.gameObject.SetActive(isExpanded);

            foreach (QuestsTrackerTask taskEntry in taskEntries.Values)
            {
                taskEntry.SetExpandedStatus(newIsExpanded);
            }

            OnLayoutRebuildRequested?.Invoke();
        }

        private void OnPinToggleValueChanged(bool isOn)
        {
            if (quest == null)
                return;

            if (!quest.canBePinned)
            {
                pinnedQuests.Remove(quest.id);
                SetPinStatus(false);
                return;
            }

            if (isOn)
            {
                if (!pinnedQuests.Contains(quest.id))
                    pinnedQuests.Add(quest.id);
            }
            else
            {
                pinnedQuests.Remove(quest.id);
            }
        }

        public void SetPinStatus(bool isPinned) { pinQuestToggle.SetIsOnWithoutNotify(isPinned); }

        public void StartDestroy() { StartCoroutine(DestroyRoutine()); }

        private IEnumerator DestroyRoutine()
        {
            containerAnimator.SetTrigger(OUT_ANIM_TRIGGER);
            yield return WaitForSecondsCache.Get(DELAY_TO_DESTROY);

            OnLayoutRebuildRequested?.Invoke();
            Destroy(gameObject);
        }

        private void Update() { progress.fillAmount = Mathf.MoveTowards(progress.fillAmount, progressTarget, 0.1f); }

        private void OnDestroy()
        {
            if (iconPromise != null)
            {
                iconPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(iconPromise);
            }
        }
    }
}