using UnityEngine;

internal class AnimationHandler
{
    private const string PARAM_LOADING_COMPLETE = "LoadingComplete";
    private static readonly int paramLoadingComplete = Animator.StringToHash(PARAM_LOADING_COMPLETE);

    private readonly Animator animator;

    public AnimationHandler(Animator animator)
    {
        this.animator = animator;
    }

    public void SetLoaded()
    {
        animator.SetTrigger(paramLoadingComplete);
    }
}
