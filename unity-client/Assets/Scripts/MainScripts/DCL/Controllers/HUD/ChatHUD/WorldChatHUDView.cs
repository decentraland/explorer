using DCL;
using UnityEngine;
using UnityEngine.UI;

public class WorldChatHUDView : MonoBehaviour
{
    public Button worldFilterButton;
    public Button pmFilterButton;
    public Button closeButton;

    public CanvasGroup group;

    private void Start()
    {
        InitialSceneReferences.i.mouseCatcher.OnMouseLock += MouseCatcher_OnMouseLock;
        InitialSceneReferences.i.mouseCatcher.OnMouseUnlock += MouseCatcher_OnMouseUnlock;
    }

    private void MouseCatcher_OnMouseUnlock()
    {
        group.alpha = 1;
    }

    private void MouseCatcher_OnMouseLock()
    {
        group.alpha = 0;
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    }
}
