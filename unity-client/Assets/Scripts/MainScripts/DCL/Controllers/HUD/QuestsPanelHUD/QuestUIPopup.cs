using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestUIPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private RectTransform sectionsContainer;
        [SerializeField] private GameObject sectionPrefab;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            closeButton.onClick.AddListener(ClosePopup);
        }

        public void Populate(QuestModel quest)
        {
            CleanUpQuestsList(); //TODO Reuse already instantiated quests

            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail_banner);
            for (int i = 0; i < quest.sections.Length; i++)
            {
                CreateTask(quest.sections[i]);
            }
            Utils.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        internal void SetThumbnail(string thumbnailURL)
        {

        }

        internal void CreateTask(QuestSection section)
        {
            var taskEntry = Instantiate(sectionPrefab, sectionsContainer).GetComponent<SectionUIEntry>();
            taskEntry.Populate(section);
        }

        internal void CleanUpQuestsList()
        {
            for(int i = sectionsContainer.childCount - 1; i >= 0; i--)
                Destroy(sectionsContainer.GetChild(i).gameObject);
        }

        internal void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}