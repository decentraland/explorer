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
        internal const float OUT_ANIM_DELAY = 1f;
        private const float DELAY_TO_DESTROY = 0.5f;
        private static readonly int OUT_ANIM_TRIGGER = Animator.StringToHash("Out");
        public event Action OnLayoutRebuildRequested;
        public event Action<QuestModel> OnQuestCompleted;
        public event Action<QuestReward> OnRewardObtained;

        [SerializeField] internal TextMeshProUGUI questTitle;
        [SerializeField] internal RawImage questIcon;
        [SerializeField] internal Image progress;
        [SerializeField] internal RectTransform sectionContainer;
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

        private float progressTarget = 0;
        private AssetPromise_Texture iconPromise;

        internal QuestModel quest;
        private bool isExpanded;
        private bool isPinned;
        private bool outAnimDone = false;

        private readonly Dictionary<string, QuestsTrackerTask> taskEntries = new Dictionary<string, QuestsTrackerTask>();
        private readonly List<QuestReward> rewardsToNotify = new List<QuestReward>();
        private readonly List<Coroutine> tasksRoutines = new List<Coroutine>();
        private Coroutine sequenceRoutine;
        private Coroutine progressRoutine;

        public void Awake()
        {
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);

            expandCollapseButton.gameObject.SetActive(false);
            SetExpandCollapseState(true);
            expandCollapseButton.onClick.AddListener(() => SetExpandCollapseState(!isExpanded));
            StartCoroutine(OutDelayRoutine());
        }

        private IEnumerator OutDelayRoutine()
        {
            yield return new WaitForSeconds(OUT_ANIM_DELAY);
            outAnimDone = true;
        }

        public void Populate(QuestModel newQuest)
        {
            ClearTaskRoutines();

            quest = newQuest;
            SetIcon(quest.thumbnail_entry);
            QuestTask[] allTasks = quest.sections.SelectMany(x => x.tasks).ToArray();

            int completedTasksAmount = allTasks.Count(x => x.progress >= 1);
            questTitle.text = $"{quest.name} - {completedTasksAmount}/{allTasks.Length}";
            progressTarget = (float)completedTasksAmount / allTasks.Length;

            bool hasCompletedTasksToShow = false;
            List<string> entriesToRemove = taskEntries.Keys.ToList();
            List<QuestsTrackerTask> visibleTaskEntries = new List<QuestsTrackerTask>();
            List<QuestsTrackerTask> newTaskEntries = new List<QuestsTrackerTask>();
            for (int index = 0; index < allTasks.Length; index++)
            {
                QuestTask task = allTasks[index];
                //We only show completed quests that has been just completed to show the progress
                if (task.status == QuestsLiterals.Status.BLOCKED || (task.progress >= 1 && !task.justProgressed))
                    continue;

                entriesToRemove.Remove(task.id);
                if (!taskEntries.TryGetValue(task.id, out QuestsTrackerTask taskEntry))
                {
                    taskEntry = CreateTask();
                    //Only completed tasks are visible when created
                    taskEntry.gameObject.SetActive(task.progress >= 1);
                    taskEntries.Add(task.id, taskEntry);
                }

                taskEntry.Populate(task);
                taskEntry.transform.SetSiblingIndex(index);

                if (taskEntry.gameObject.activeSelf)
                    visibleTaskEntries.Add(taskEntry);
                else
                    newTaskEntries.Add(taskEntry);

                if (task.progress >= 1)
                    hasCompletedTasksToShow = true;
            }

            for (int index = 0; index < entriesToRemove.Count; index++)
            {
                DestroyTaskEntry(entriesToRemove[index]);
            }

            expandCollapseButton.gameObject.SetActive(taskEntries.Count > 0);
            SetExpandCollapseState(true);
            OnLayoutRebuildRequested?.Invoke();

            if (sequenceRoutine != null)
                StopCoroutine(sequenceRoutine);
            sequenceRoutine = StartCoroutine(Sequence(visibleTaskEntries, newTaskEntries, hasCompletedTasksToShow));
        }

        internal QuestsTrackerTask CreateTask()
        {
            var taskEntry = Instantiate(taskPrefab, sectionContainer).GetComponent<QuestsTrackerTask>();
            taskEntry.OnRequestLayoutRebuild += () => OnLayoutRebuildRequested?.Invoke();
            taskEntry.OnDestroyed += (taskId) => taskEntries.Remove(taskId);
            return taskEntry;
        }

        private void DestroyTaskEntry(string taskId)
        {
            if (!taskEntries.TryGetValue(taskId, out QuestsTrackerTask taskEntry))
                return;
            Destroy(taskEntry.gameObject);
            taskEntries.Remove(taskId);
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
            iconPromise.OnSuccessEvent += assetTexture => iconImage.texture = assetTexture.texture;
            iconPromise.OnFailEvent += assetTexture => { Debug.Log($"Error downloading quest tracker entry icon: {iconURL}"); };

            AssetPromiseKeeper_Texture.i.Keep(iconPromise);
        }

        private IEnumerator Sequence(List<QuestsTrackerTask> visibleTasks, List<QuestsTrackerTask> newTasks, bool hasCompletedTasks)
        {
            if (hasCompletedTasks)
                yield return new WaitUntil(() => outAnimDone);

            ClearTaskRoutines();

            if (progressRoutine != null)
                StopCoroutine(progressRoutine);
            progressRoutine = StartCoroutine(ProgressSequence());

            //Progress of currently visible tasks
            for (int i = 0; i < visibleTasks.Count; i++)
            {
                tasksRoutines.Add(StartCoroutine(visibleTasks[i].ProgressAndCompleteSequence()));
            }
            yield return WaitForTaskRoutines();
            OnLayoutRebuildRequested?.Invoke();

            //Show and progress of new tasks
            for (int i = 0; i < newTasks.Count; i++)
            {
                newTasks[i].gameObject.SetActive(true);
                tasksRoutines.Add(StartCoroutine(newTasks[i].ProgressAndCompleteSequence()));
            }
            OnLayoutRebuildRequested?.Invoke();
            yield return WaitForTaskRoutines();

            if (progressRoutine != null)
                yield return progressRoutine;

            //The entry should exit automatically if questCompleted, therefore the use of MinValue
            DateTime tasksIdleTime = quest.isCompleted ? DateTime.MinValue : DateTime.Now;
            yield return new WaitUntil(() => isProgressAnimationDone && !isPinned && (DateTime.Now - tasksIdleTime) > TimeSpan.FromSeconds(3));

            if (quest.isCompleted)
                OnQuestCompleted?.Invoke(quest);

            for (int i = 0; i < rewardsToNotify.Count; i++)
            {
                OnRewardObtained?.Invoke(rewardsToNotify[i]);
            }
            rewardsToNotify.Clear();

            isReadyForDisposal = true;
        }

        private void ClearTaskRoutines()
        {
            if (tasksRoutines.Count > 0)
            {
                for (int i = 0; i < tasksRoutines.Count; i++)
                {
                    if (tasksRoutines[i] != null)
                        StopCoroutine(tasksRoutines[i]);
                }
                tasksRoutines.Clear();
            }
        }

        internal void SetExpandCollapseState(bool newIsExpanded)
        {
            isExpanded = newIsExpanded;
            expandIcon.SetActive(!isExpanded);
            collapseIcon.SetActive(isExpanded);
            sectionContainer.gameObject.SetActive(isExpanded);

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

        public void AddRewardToGive(QuestReward reward) { rewardsToNotify.Add(reward); }

        public void StartDestroy() { StartCoroutine(DestroySequence()); }

        private IEnumerator DestroySequence()
        {
            containerAnimator.SetTrigger(OUT_ANIM_TRIGGER);
            yield return WaitForSecondsCache.Get(DELAY_TO_DESTROY);

            OnLayoutRebuildRequested?.Invoke();
            Destroy(gameObject);
        }

        private IEnumerator ProgressSequence()
        {
            while (progress.fillAmount < progressTarget)
            {
                progress.fillAmount = Mathf.MoveTowards(progress.fillAmount, progressTarget, 2f * Time.deltaTime);
                yield return null;
            }
            progressRoutine = null;
        }

        private IEnumerator WaitForTaskRoutines()
        {
            for (int i = 0; i < tasksRoutines.Count; i++)
            {
                //yielding Coroutines (not IEnumerators) allows us to wait for them in parallel
                yield return tasksRoutines[i];
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
    }
}