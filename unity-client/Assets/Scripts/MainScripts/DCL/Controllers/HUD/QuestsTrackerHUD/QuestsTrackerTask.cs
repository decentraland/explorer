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
        public bool isReadyForCompletion { get; private set; } = false;

        public void Awake()
        {
            jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke(); });
            StartCoroutine(DisposalRoutine());
        }

        public void Populate(QuestTask newTask)
        {
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
            if (!isProgressAnimationDone)
            {
                Vector3 scale = progress.transform.localScale;
                scale.x = Mathf.MoveTowards(scale.x, progressTarget, 2f * Time.deltaTime);
                progress.transform.localScale = scale;
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

        public void StartCompletion(Action onDone)
        {
            isCompleting = true;
            StopAllCoroutines();
            StartCoroutine(CompletionRoutine(onDone));
        }

        private IEnumerator CompletionRoutine(Action onDone)
        {
            yield return WaitForSecondsCache.Get(0.2f);
            animator.SetTrigger("Completed");
            yield return WaitForSecondsCache.Get(2f);
            Destroy(gameObject);
            onDone?.Invoke();
            isCompleting = false;
        }

        private IEnumerator DisposalRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.25f);
                if (task == null)
                    continue;
                if (!isProgressAnimationDone)
                    continue;
                if (task.progress < 1)
                    continue;

                isReadyForCompletion = true;
            }
        }
    }
}