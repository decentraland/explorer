using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestPanel
{
    public class QuestUIEntry : MonoBehaviour
    {
        public event Action<QuestPanelModel> OnReadMoreClicked;

        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Button readMoreButton;

        internal Action readMoreDelegate;

        private void Awake()
        {
            readMoreButton.onClick.AddListener(() => readMoreDelegate?.Invoke());
        }

        public void Populate(QuestPanelModel quest)
        {
            readMoreDelegate = () => OnReadMoreClicked?.Invoke(quest);
            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail_entry);
        }

        internal void SetThumbnail(string thumbnailURL)
        {

        }
    }
}