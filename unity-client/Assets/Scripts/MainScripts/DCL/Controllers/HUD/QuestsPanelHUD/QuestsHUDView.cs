using DCL.Helpers;
using UnityEngine;

namespace DCL.Huds.QuestPanel
{
    public class QuestsHUDView : MonoBehaviour
    {
        private const string VIEW_PATH = "QuestsHUD";

        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private GameObject questPrefab;
        [SerializeField] private QuestUIPopup questPopup;

        internal static QuestsHUDView Create()
        {
            var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<QuestsHUDView>();
#if UNITY_EDITOR
            view.gameObject.name = "_QuestHUDView";
#endif
            return view;
        }

        public void Awake()
        {
            questPopup.gameObject.SetActive(false);
        }

        public void Populate(QuestPanelModel[] quests)
        {
            CleanUpQuestsList(); //TODO Reuse already instantiated quests
            for (int i = 0; i < quests.Length; i++)
            {
                CreateQuestEntry(quests[i]);
            }
        }

        internal void CreateQuestEntry(QuestPanelModel quest)
        {
            var questEntry = Instantiate(questPrefab, questsContainer).GetComponent<QuestUIEntry>();
            questEntry.OnReadMoreClicked += ShowQuestPopup;
            questEntry.Populate(quest);
        }

        internal void ShowQuestPopup(QuestPanelModel quest)
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