using DCL.Interface;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerTask : MonoBehaviour
    {
        private static readonly int EXPAND_ANIMATOR_TRIGGER = Animator.StringToHash("Expand");
        private static readonly int COLLAPSE_ANIMATOR_TRIGGER = Animator.StringToHash("Collapse");

        public event Action<string> OnCompleted;
        public event Action<string> OnDestroyed;
        public event Action OnRequestLayoutRebuild;

        [SerializeField] internal TextMeshProUGUI taskTitle;
        [SerializeField] internal Image progress;
        [SerializeField] internal TextMeshProUGUI progressText;
        [SerializeField] internal Button jumpInButton;
        [SerializeField] internal Animator animator;

        private float progressTarget = 0;
        private bool isProgressAnimationDone => Math.Abs(progressTarget - progress.transform.localScale.x) < Mathf.Epsilon;

        private QuestTask task = null;

        private Action jumpInDelegate;
        public bool isIdle => isProgressAnimationDone && !isCompleting;
        public bool isCompleting { get; private set; } = false;
        public bool enableProgress = false;

        public void Awake() { jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke(); }); }

        public void Populate(QuestTask newTask)
        {
            StopAllCoroutines();
            isCompleting = false;
            task = newTask;
            taskTitle.text = task.name;
            jumpInDelegate = () => WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = $"/goto {task.coordinates}",
            });

            jumpInButton.gameObject.SetActive(task.progress < 1 && !string.IsNullOrEmpty(task.coordinates));
            progressTarget = task.progress;
            switch (task.type)
            {
                case "single":
                    SetProgressText(task.progress, 1);
                    break;
                case "numeric":
                    var payload = JsonUtility.FromJson<TaskPayload_Numeric>(task.payload);
                    SetProgressText(payload.current, payload.end);
                    break;
            }
        }

        private void Update()
        {
            if (!enableProgress)
                return;

            if (!isProgressAnimationDone)
            {
                Vector3 scale = progress.transform.localScale;
                scale.x = Mathf.MoveTowards(scale.x, progressTarget, 2f * Time.deltaTime);
                progress.transform.localScale = scale;
                if (progressTarget >= 1 && !isCompleting)
                    StartCompletion();
            }
        }

        internal void SetProgressText(float current, float end) { progressText.text = $"{current}/{end}"; }

        public void SetExpandedStatus(bool active)
        {
            if (active)
                animator.SetTrigger(EXPAND_ANIMATOR_TRIGGER);
            else
                animator.SetTrigger(COLLAPSE_ANIMATOR_TRIGGER);
        }

        private void StartCompletion()
        {
            isCompleting = true;
            StopAllCoroutines();
            StartCoroutine(CompletionRoutine());
        }

        private IEnumerator CompletionRoutine()
        {
            yield return WaitForSecondsCache.Get(0.5f);
            animator.SetTrigger("Completed");

            yield return WaitForSecondsCache.Get(2f);

            //Note Alex: We set the height to 0 to hide the entry. Disabling it would end up on the coroutine being stuck
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta *= Vector2.right;
            OnRequestLayoutRebuild?.Invoke();
            OnCompleted?.Invoke(task.id);
            yield return WaitForSecondsCache.Get(0.5f);

            isCompleting = false;
            OnDestroyed?.Invoke(task.id);
            Destroy(gameObject);
        }
    }
}