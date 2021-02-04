using DCL.Helpers;
using DCL.Interface;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelEntry : MonoBehaviour
    {
        public event Action<string> OnReadMoreClicked;

        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Button readMoreButton;
        [SerializeField] private Toggle pinQuestToggle;
        [SerializeField] private Image progressInTitle;
        [SerializeField] private RectTransform completedProgressInTitle;
        [SerializeField] private RectTransform completedMarkInTitle;
        [SerializeField] private RawImage thumbnailImage;
        [SerializeField] private Button jumpInButton;

        private AssetPromise_Texture thumbnailPromise;

        private QuestModel quest;

        internal Action readMoreDelegate;
        private static BaseCollection<string> pinnedQuests => DataStore.Quests.pinnedQuests;

        private Action jumpInDelegate;

        private void Awake()
        {
            jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke();});
            readMoreButton.onClick.AddListener(() => readMoreDelegate?.Invoke());
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);
            pinnedQuests.OnAdded += OnPinnedQuests;
            pinnedQuests.OnRemoved += OnUnpinnedQuest;
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;

            QuestTask incompletedTask = quest.sections.FirstOrDefault(x => x.progress < 1)?.tasks.FirstOrDefault(x => x.progress < 1);
            jumpInButton.gameObject.SetActive(incompletedTask != null && !string.IsNullOrEmpty(incompletedTask?.coordinates));
            jumpInDelegate = () => WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = $"/goto {incompletedTask?.coordinates}",
            });

            readMoreDelegate = () => OnReadMoreClicked?.Invoke(quest.id);
            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail_entry);
            pinQuestToggle.SetIsOnWithoutNotify(pinnedQuests.Contains(quest.id));

            var questCompleted = quest.isCompleted;
            pinQuestToggle.gameObject.SetActive(!questCompleted);
            progressInTitle.fillAmount = quest.progress;
            completedProgressInTitle.gameObject.SetActive(questCompleted);
            completedMarkInTitle.gameObject.SetActive(questCompleted);
        }

        private void OnPinToggleValueChanged(bool isOn)
        {
            if (quest == null)
                return;

            if (quest.isCompleted)
            {
                pinnedQuests.Remove(quest.id);
                pinQuestToggle.SetIsOnWithoutNotify(false);
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
            thumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading quest panel entry thumbnail: {thumbnailURL}"); };

            AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
        }

        private void OnThumbnailReady(Asset_Texture assetTexture)
        {
            thumbnailImage.texture = assetTexture.texture;
        }

        private void OnDestroy()
        {
            pinnedQuests.OnAdded -= OnUnpinnedQuest;
            pinnedQuests.OnRemoved -= OnPinnedQuests;
        }
    }
}