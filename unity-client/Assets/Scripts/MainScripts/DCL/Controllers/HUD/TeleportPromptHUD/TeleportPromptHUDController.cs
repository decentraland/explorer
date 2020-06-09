using UnityEngine;

public class TeleportPromptHUDController : IHUD
{
    internal TeleportPromptHUDView view { get; private set; }

    public TeleportPromptHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("TeleportPromptHUD")).GetComponent<TeleportPromptHUDView>();
        view.name = "_TeleportPromptHUD";
        view.content.SetActive(false);
    }

    public void SetVisibility(bool visible)
    {
        if (view.content.activeSelf && !visible)
        {
            view.Hide();
        }
        else if (!view.content.activeSelf && visible)
        {
            view.content.SetActive(true);
        }
    }

    public void RequestTeleport(string destination)
    {
        if (!view.content.activeSelf)
        {
            view.Teleport(destination);
        }
    }

    public void Dispose()
    {
        if (view)
        {
            Object.Destroy(view);
        }
    }
}
