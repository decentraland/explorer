using DCL.Helpers;
using UnityEngine;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelHUDView : MonoBehaviour
    {
        private const string VIEW_PATH = "QuestsHUD";

        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private GameObject questPrefab;
        [SerializeField] private QuestPanelPopup questPopup;

        internal static QuestsPanelHUDView Create()
        {
            var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<QuestsPanelHUDView>();
#if UNITY_EDITOR
            view.gameObject.name = "_QuestHUDView";
#endif
            return view;
        }

        public void Awake()
        {
            questPopup.gameObject.SetActive(false);
        }

        public void Populate(QuestModel[] quests)
        {
            CleanUpQuestsList(); //TODO Reuse already instantiated quests
            for (int i = 0; i < quests.Length; i++)
            {
                CreateQuestEntry(quests[i]);
            }
        }

        internal void CreateQuestEntry(QuestModel quest)
        {
            var questEntry = Instantiate(questPrefab, questsContainer).GetComponent<QuestPanelEntry>();
            questEntry.OnReadMoreClicked += ShowQuestPopup;
            questEntry.Populate(quest);
        }

        internal void ShowQuestPopup(QuestModel quest)
        {
            questPopup.Populate(quest);
            questPopup.gameObject.SetActive(true);
        }

        internal void CleanUpQuestsList()
        {
            while (questsContainer.childCount > 0)
            {
                Destroy(questsContainer.GetChild(0).gameObject);
            }
        }
    }
}