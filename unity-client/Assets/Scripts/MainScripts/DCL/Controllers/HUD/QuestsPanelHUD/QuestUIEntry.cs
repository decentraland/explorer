using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestUIEntry : MonoBehaviour
    {
        public event Action<QuestModel> OnReadMoreClicked;

        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Button readMoreButton;

        internal Action readMoreDelegate;

        private void Awake()
        {
            readMoreButton.onClick.AddListener(() => readMoreDelegate?.Invoke());
        }

        public void Populate(QuestModel quest)
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