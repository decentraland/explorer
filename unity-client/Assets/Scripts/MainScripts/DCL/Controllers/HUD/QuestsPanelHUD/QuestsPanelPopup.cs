using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private RectTransform sectionsContainer;
        [SerializeField] private GameObject sectionPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Toggle pinQuestToggle;
        [SerializeField] private RawImage thumbnailImage;

        private AssetPromise_Texture thumbnailPromise;

        private QuestModel quest;
        private readonly List<QuestsPanelSection> sections = new List<QuestsPanelSection>();
        private static BaseCollection<string> baseCollection => DataStore.Quests.pinnedQuests;

        private void Awake()
        {
            closeButton.onClick.AddListener(Close);

            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);
            baseCollection.OnAdded += OnPinnedQuests;
            baseCollection.OnRemoved += OnUnpinnedQuest;
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;
            PrepareSections(quest.sections.Length);

            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail_banner);
            for (int i = 0; i < quest.sections.Length; i++)
            {
                sections[i].Populate(quest.sections[i]);
            }
            Utils.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            pinQuestToggle.SetIsOnWithoutNotify(baseCollection.Contains(quest.id));
            pinQuestToggle.gameObject.SetActive(!quest.isCompleted);
        }

        private void OnPinToggleValueChanged(bool isOn)
        {
            if (quest == null)
                return;

            if (isOn)
            {
                if (!baseCollection.Contains(quest.id))
                    baseCollection.Add(quest.id);
            }
            else
            {
                if (baseCollection.Contains(quest.id))
                    baseCollection.Remove(quest.id);
            }
        }

        private void OnPinnedQuests(string questId)
        {
            if (quest != null && quest.id == questId)
                pinQuestToggle.SetIsOnWithoutNotify(true);
        }

        private void OnUnpinnedQuest(string questId)
        {
            if (quest != null && quest.id == questId)
                pinQuestToggle.SetIsOnWithoutNotify(false);
        }

        internal void SetThumbnail(string thumbnailURL)
        {
            if (thumbnailPromise != null)
            {
                thumbnailPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            }

            if (string.IsNullOrEmpty(thumbnailURL))
                return;

            thumbnailPromise = new AssetPromise_Texture(thumbnailURL);
            thumbnailPromise.OnSuccessEvent += OnThumbnailReady;
            thumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading quest panel popup thumbnail: {thumbnailURL}"); };

            AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
        }

        private void OnThumbnailReady(Asset_Texture assetTexture)
        {
            thumbnailImage.texture = assetTexture.texture;
        }

        internal void CreateSection()
        {
            sections.Add(Instantiate(sectionPrefab, sectionsContainer).GetComponent<QuestsPanelSection>());
        }

        internal void PrepareSections(int sectionsAmount)
        {
            if (sections.Count == sectionsAmount)
                return;

            if (sections.Count < sectionsAmount)
            {
                while(sections.Count < sectionsAmount)
                    CreateSection();
            }
            else
            {
                while (sections.Count > sectionsAmount)
                {
                    var section = sections.Last();
                    sections.RemoveAt(sections.Count - 1);
                    Destroy(section.gameObject);
                }
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}