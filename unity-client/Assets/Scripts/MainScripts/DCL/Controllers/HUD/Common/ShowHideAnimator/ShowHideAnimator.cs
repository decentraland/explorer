using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShowHideAnimator : MonoBehaviour
{
    public event System.Action OnWillFinishHide;
    public event System.Action OnWillFinishStart;

    public bool disableAfterFadeOut;
    public string visibleParam = "visible";

    private Animator animatorValue;
    private Animator animator
    {
        get
        {
            if (animatorValue == null)
            {
                animatorValue = GetComponent<Animator>();
            }

            return animatorValue;
        }
    }
    private int? visibleParamHashValue = null;

    private int visibleParamHash
    {
        get
        {
            if (!visibleParamHashValue.HasValue)
                visibleParamHashValue = Animator.StringToHash(visibleParam);

            return visibleParamHashValue.Value;
        }
    }

    public void Show()
    {
        animator.SetBool(visibleParamHash, true);
    }

    public void Hide()
    {
        animator.SetBool(visibleParamHash, false);
    }


    public void AnimEvent_HideFinished()
    {
        OnWillFinishHide?.Invoke();

        if (disableAfterFadeOut)
        {
            gameObject.SetActive(false);
        }
    }

    public void AnimEvent_ShowFinished()
    {
        OnWillFinishStart?.Invoke();
    }
}
