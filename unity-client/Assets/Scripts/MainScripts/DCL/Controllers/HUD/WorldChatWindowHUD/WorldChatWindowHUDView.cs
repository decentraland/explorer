using DCL;
using UnityEngine;
using UnityEngine.UI;

public class WorldChatWindowHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Main Chat Window";

    public Button worldFilterButton;
    public Button pmFilterButton;
    public Button closeButton;

    public ChatHUDView chatHudView;

    public CanvasGroup group;
    public WorldChatWindowHUDController controller;

    public static WorldChatWindowHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<WorldChatWindowHUDView>();
        return view;
    }

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
