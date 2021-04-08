using System;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public interface IQuestsPanelHUDView
    {
        void RequestAddOrUpdateQuest(string questId);
        void RemoveQuest(string questId);
        void ClearQuests();
        void SetVisibility(bool active);
        bool isVisible { get; }
        void Dispose();
    }

    public class QuestsPanelHUDView : MonoBehaviour, IQuestsPanelHUDView
    {
        internal static int ENTRIES_PER_FRAME { get; set; } = 5;
        private const string VIEW_PATH = "QuestsPanelHUD";

        [SerializeField] private RectTransform availableQuestsContainer;
        [SerializeField] private RectTransform completedQuestsContainer;
        [SerializeField] private GameObject questsContainerSeparators;
        [SerializeField] private GameObject questPrefab;
        [SerializeField] internal QuestsPanelPopup questPopup;
        [SerializeField] private Button closeButton;
        [SerializeField] private DynamicScrollSensitivity dynamicScrollSensitivity;

        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;

        private string currentQuestInPopup = "";
        internal readonly Dictionary<string, QuestsPanelEntry> questEntries =  new Dictionary<string, QuestsPanelEntry>();
        private bool orderQuestsRequested = false;
        private bool layoutRebuildRequested = false;
        internal readonly List<string> questsToBeAdded = new List<string>();
        private bool isDestroyed = false;

        internal static QuestsPanelHUDView Create()
        {
            var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<QuestsPanelHUDView>();
#if UNITY_EDITOR
            view.gameObject.name = "_QuestsPanelHUDView";
#endif
            return view;
        }

        public void Awake()
        {
            questPopup.gameObject.SetActive(false);
            closeButton.onClick.AddListener(() => DataStore.i.HUDs.questsPanelVisible.Set(false));
        }

        public void RequestAddOrUpdateQuest(string questId)
        {
            if (questsToBeAdded.Contains(questId))
            {
                AddOrUpdateQuest(questId);
                return;
            }

            questsToBeAdded.Add(questId);
        }

        internal void AddOrUpdateQuest(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest))
            {
                Debug.LogError($"Couldn't find quest with ID {questId} in DataStore");
                return;
            }

            //Quest has no available tasks, we remove it.
            if (!quest.hasAvailableTasks)
            {
                RemoveQuest(questId);
                return;
            }

            if (!questEntries.TryGetValue(questId, out QuestsPanelEntry questEntry))
            {
                questEntry = Instantiate(questPrefab).GetComponent<QuestsPanelEntry>();
                questEntry.OnReadMoreClicked += ShowQuestPopup;
                questEntries.Add(questId, questEntry);
            }

            questEntry.transform.localScale = Vector3.one;
            questEntry.Populate(quest);
            if (currentQuestInPopup == questId)
                questPopup.Populate(quest);

            orderQuestsRequested = true;
            layoutRebuildRequested = true;
        }

        public void RemoveQuest(string questId)
        {
            questsToBeAdded.Remove(questId);

            if (!questEntries.TryGetValue(questId, out QuestsPanelEntry questEntry))
                return;
            questEntries.Remove(questId);
            questEntry.transform.SetParent(null);
            Destroy(questEntry.gameObject);

            if (currentQuestInPopup == questId)
                questPopup.Close();

            questsContainerSeparators.SetActive(completedQuestsContainer.childCount > 0);
            layoutRebuildRequested = true;
        }

        public void ClearQuests()
        {
            questPopup.Close();
            foreach (QuestsPanelEntry questEntry in questEntries.Values)
            {
                questEntry.transform.SetParent(null);
                Destroy(questEntry.gameObject);
            }
            questEntries.Clear();
            questsToBeAdded.Clear();
            questsContainerSeparators.SetActive(completedQuestsContainer.childCount > 0);
            layoutRebuildRequested = true;
        }

        internal void ShowQuestPopup(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest))
            {
                Debug.Log($"Couldnt find quest with id {questId}");
                return;
            }

            Vector3 pos = questPopup.transform.position;
            pos.y = questEntries[questId].readMorePosition.y;
            questPopup.transform.position = pos;

            currentQuestInPopup = questId;
            questPopup.Populate(quest);
            questPopup.Show();
        }

        internal void Update()
        {
            if (layoutRebuildRequested)
            {
                layoutRebuildRequested = false;
                Utils.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                dynamicScrollSensitivity?.RecalculateSensitivity();
            }

            if (orderQuestsRequested)
            {
                orderQuestsRequested = false;
                OrderQuests();
            }

            for (int i = 0; i < ENTRIES_PER_FRAME && questsToBeAdded.Count > 0; i++)
            {
                string questId = questsToBeAdded.First();
                questsToBeAdded.RemoveAt(0);
                AddOrUpdateQuest(questId);
            }
        }

        private void OrderQuests()
        {
            var questModels = questEntries.Keys.Select(x => quests.Get(x));

            string[] availableIdsSorted = questModels.Where(x => !x.isCompleted).OrderBy(x => x.assignmentTime).ThenBy(x => x.id).Select(x => x.id).ToArray();
            for (int i = 0; i < availableIdsSorted.Length; i++)
            {
                questEntries[availableIdsSorted[i]].transform.SetParent(availableQuestsContainer);
                questEntries[availableIdsSorted[i]].transform.localScale = Vector3.one;
                questEntries[availableIdsSorted[i]].transform.SetSiblingIndex(i);
            }

            string[] completedQuestsSorted = questModels.Where(x => x.isCompleted).OrderBy(x => x.completionTime).ThenBy(x => x.id).Select(x => x.id).ToArray();
            for (int i = 0; i < completedQuestsSorted.Length; i++)
            {
                questEntries[completedQuestsSorted[i]].transform.SetParent(completedQuestsContainer);
                questEntries[completedQuestsSorted[i]].transform.localScale = Vector3.one;
                questEntries[completedQuestsSorted[i]].transform.SetSiblingIndex(i);
            }

            questsContainerSeparators.SetActive(completedQuestsContainer.childCount > 0);
        }

        public void SetVisibility(bool active) { gameObject.SetActive(active); }

        public bool isVisible => gameObject.activeSelf;

        public void Dispose()
        {
            if (!isDestroyed)
                Destroy(gameObject);
        }

        private void OnDestroy() { isDestroyed = true; }

        private int fakeFlowStep = 0;
        private QuestModel defaultFakeQuest = new QuestModel
        {
            id = "fake",
            assignmentTime = DateTime.MinValue,
            completionTime = DateTime.MinValue,
            name = "fake quest",
            description = "my fake quest",
            status = QuestsLiterals.Status.ON_GOING,
            sections = new []
            {
                new QuestSection
                {
                    id = "fakeSection0",
                    name = "fake section 0",
                    progress = 0,
                    tasks = new []
                    {
                        new QuestTask
                        {
                            id = "fakeTask0_0",
                            name = "fake task 0_0",
                            completionTime = DateTime.Now,
                            coordinates = "0,0",
                            progress = 0,
                            status = QuestsLiterals.Status.ON_GOING,
                            payload = JsonUtility.ToJson(new TaskPayload_Single { isDone = false }),
                            type = "single",
                        },
                        new QuestTask
                        {
                            id = "fakeTask0_1",
                            name = "fake task 0_1",
                            completionTime = DateTime.Now,
                            progress = 0,
                            status = QuestsLiterals.Status.BLOCKED,
                            payload = JsonUtility.ToJson(new TaskPayload_Numeric() { current = 0, end = 10, start = 0 }),
                            type = "numeric",
                        }
                    }
                },
                new QuestSection
                {
                    id = "fakeSection1",
                    name = "fake section 1",
                    progress = 0,
                    tasks = new []
                    {
                        new QuestTask
                        {
                            id = "fakeTask1_0",
                            name = "fake task 1_0",
                            completionTime = DateTime.Now,
                            coordinates = "1,0",
                            progress = 0,
                            status = QuestsLiterals.Status.BLOCKED,
                            payload = JsonUtility.ToJson(new TaskPayload_Single { isDone = false }),
                            type = "single",
                        }
                    }
                }
            },
            rewards = new []
            {
                new QuestReward
                {
                    id = "fakeReward0",
                    name = "fake reward 0",
                    status = QuestsLiterals.RewardStatus.OK,
                    imageUrl = "https://picsum.photos/200/300"
                },
                new QuestReward
                {
                    id = "fakeReward1",
                    name = "fake reward 1",
                    status = QuestsLiterals.RewardStatus.OK,
                    imageUrl = "https://picsum.photos/200/301"
                }
            }
        };
        private QuestModel currentFakeQuest;
        [ContextMenu("Next Step")]
        public void FakeFlow()
        {
            switch (fakeFlowStep)
            {
                //Add default quest
                case 0:
                    quests.Remove(defaultFakeQuest.id);
                    currentFakeQuest = JsonUtility.FromJson<QuestModel>(JsonUtility.ToJson(defaultFakeQuest));
                    break;
                //section 0, task 0 completed
                case 1:
                    currentFakeQuest.sections[0].tasks[0].progress = 1;
                    currentFakeQuest.sections[0].tasks[0].payload = JsonUtility.ToJson(new TaskPayload_Single { isDone = true });
                    currentFakeQuest.sections[0].tasks[1].status = QuestsLiterals.Status.ON_GOING;
                    currentFakeQuest.sections[0].progress = 0.5f;
                    break;
                //section 0, task 1 halfway
                case 2:
                    currentFakeQuest.sections[0].tasks[1].progress = 0.5f;
                    currentFakeQuest.sections[0].tasks[1].payload = JsonUtility.ToJson(new TaskPayload_Numeric() { current = 5, end = 10, start = 0 });
                    currentFakeQuest.sections[0].progress = 0.75f;
                    break;
                //section 0, task 1 completed
                case 3:
                    currentFakeQuest.sections[0].tasks[1].progress = 1f;
                    currentFakeQuest.sections[0].tasks[1].payload = JsonUtility.ToJson(new TaskPayload_Numeric() { current = 10, end = 10, start = 0 });
                    currentFakeQuest.sections[0].progress = 1;
                    currentFakeQuest.sections[1].tasks[0].status = QuestsLiterals.Status.ON_GOING;

                    break;
                //section 1, task 0 completed and quest completed
                case 4:
                    currentFakeQuest.sections[1].tasks[0].progress = 1;
                    currentFakeQuest.sections[1].tasks[0].payload = JsonUtility.ToJson(new TaskPayload_Single { isDone = true });
                    currentFakeQuest.status = QuestsLiterals.Status.COMPLETED;
                    currentFakeQuest.rewards[0].status = QuestsLiterals.RewardStatus.ALREADY_GIVEN;
                    currentFakeQuest.rewards[1].status = QuestsLiterals.RewardStatus.ALREADY_GIVEN;
                    currentFakeQuest.sections[1].progress = 1;
                    break;
            }
            fakeFlowStep = (fakeFlowStep + 1) % 5;
            QuestsController.QuestsController.i.UpdateQuestProgress(JsonUtility.FromJson<QuestModel>(JsonUtility.ToJson(currentFakeQuest)));
        }
    }
}