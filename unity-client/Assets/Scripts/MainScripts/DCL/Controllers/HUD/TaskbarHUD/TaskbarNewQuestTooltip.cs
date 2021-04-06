using DCL;
using UnityEngine;

public class TaskbarNewQuestTooltip : MonoBehaviour
{
    private static readonly int ANIM_STATE_TRIGGER = Animator.StringToHash("ShowDisabledTooltip");
    [SerializeField] private Animator animator;

    private void Awake() { DataStore.i.Quests.quests.OnAdded += OnQuestsAdded; }

    private void OnQuestsAdded(string s, QuestModel model)
    {
        if (!model.isCompleted)
            animator?.SetTrigger(ANIM_STATE_TRIGGER);
    }
}