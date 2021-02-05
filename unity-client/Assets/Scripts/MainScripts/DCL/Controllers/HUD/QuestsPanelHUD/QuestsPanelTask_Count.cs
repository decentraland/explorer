using DCL.Interface;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelTask_Count : MonoBehaviour, IQuestsPanelTask
    {
        [SerializeField] private TextMeshProUGUI stepName;
        [SerializeField] private TextMeshProUGUI start;
        [SerializeField] private TextMeshProUGUI end;
        [SerializeField] private TextMeshProUGUI current;
        [SerializeField] private Image ongoingProgress;
        [SerializeField] private Button jumpInButton;

        internal TaskPayload_Numeric payload;
        private Action jumpInDelegate;

        public void Awake()
        {
            jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke();});
        }

        public void Populate(QuestTask task)
        {
            payload = JsonUtility.FromJson<TaskPayload_Numeric>(task.payload);

            jumpInButton.gameObject.SetActive(task.progress < 1 && !string.IsNullOrEmpty(task.coordinates));
            jumpInDelegate = () => WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = $"/goto {task.coordinates}",
            });

            stepName.text = task.name;
            start.text = payload.start.ToString();
            current.text = payload.current.ToString();
            end.text = payload.end.ToString();

            ongoingProgress.fillAmount = (float)payload.current / payload.end;
        }
    }
}