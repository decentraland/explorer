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

        public bool isReadyForDisposal { get; private set; } = false;

        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;
        private bool isProgressAnimationDone => Math.Abs(progress.fillAmount - progressTarget) < Mathf.Epsilon;
        private bool isLastUpdateDelayDone => (DateTime.Now - lastUpdateTime) > TimeSpan.FromSeconds(3);

        private float progressTarget = 0;
        private AssetPromise_Texture iconPromise;

        internal QuestModel quest;
        private bool isExpanded;
        private bool isPinned;
        private DateTime lastUpdateTime = DateTime.MaxValue;

        private readonly Dictionary<string, QuestsTrackerTask> taskEntries = new Dictionary<string, QuestsTrackerTask>();

        public void Awake()
        {
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);

            expandCollapseButton.gameObject.SetActive(false);
            SetExpandCollapseState(true);
            expandCollapseButton.onClick.AddListener(() => SetExpandCollapseState(!isExpanded));
            StartCoroutine(DisposalRoutine());
            StartCoroutine(RemoveTasksRoutine());
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;
            lastUpdateTime = DateTime.Now;
            questTitle.text = quest.name;
            SetIcon(quest.thumbnail_entry);
            var availableTasks = quest.sections.SelectMany(x => x.tasks).Where(x => x.status != QuestsLiterals.Status.BLOCKED).ToArray();
            int totalTasks = availableTasks.Count();
            int completedTasks = availableTasks.Count(x => x.progress >= 1);
            QuestTask[] tasksToShow = availableTasks.Where(x => x.status != QuestsLiterals.Status.BLOCKED && (x.justProgressed || x.progress < 1))
                                                    .ToArray();
            sectionTitle.text = $"{completedTasks}/{totalTasks}";
            progressTarget = (float)completedTasks / totalTasks;

            //Initialize it with all the task and purge the list as we process the new ones.
            List<string> taskToRemove = taskEntries.Keys.ToList();

            for (int index = 0; index < tasksToShow.Length; index++)
            {
                QuestTask task = tasksToShow[index];
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
            if (!taskEntries.TryGetValue(task.id, out QuestsTrackerTask taskEntry))
            {
                taskEntry = Instantiate(taskPrefab, tasksContainer).GetComponent<QuestsTrackerTask>();
                taskEntries.Add(task.id, taskEntry);
            }
            taskEntry.Populate(task);
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

        public void SetPinStatus(bool newIsPinned)
        {
            isPinned = newIsPinned;
            pinQuestToggle.SetIsOnWithoutNotify(newIsPinned);
        }

        public void StartDestroy() { StartCoroutine(DestroyRoutine()); }

        private IEnumerator DestroyRoutine()
        {
            containerAnimator.SetTrigger(OUT_ANIM_TRIGGER);
            yield return WaitForSecondsCache.Get(DELAY_TO_DESTROY);

            OnLayoutRebuildRequested?.Invoke();
            Destroy(gameObject);
        }

        private void Update()
        {
            if (!isProgressAnimationDone)
            {
                progress.fillAmount = Mathf.MoveTowards(progress.fillAmount, progressTarget, 2f * Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (iconPromise != null)
            {
                iconPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(iconPromise);
            }
        }

        private IEnumerator DisposalRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.25f);
                if (quest == null)
                    continue;
                if (!isProgressAnimationDone)
                    continue;
                if (isPinned)
                    continue;
                if (!isLastUpdateDelayDone)
                    continue;
                if (taskEntries.Values.Any(x => !x.isIdle))
                    continue;

                isReadyForDisposal = true;
            }
        }

        private IEnumerator RemoveTasksRoutine()
        {
            while (true)
            {
                string[] entriesToRemove = taskEntries.Where(x => x.Value.isReadyForCompletion && !x.Value.isCompleting).Select(x => x.Key).ToArray();
                foreach (string taskId in entriesToRemove)
                {
                    taskEntries[taskId]
                        .StartCompletion(() =>
                        {
                            taskEntries.Remove(taskId);
                            OnLayoutRebuildRequested?.Invoke();
                        });
                }
                yield return WaitForSecondsCache.Get(0.25f);
            }
        }
    }
}