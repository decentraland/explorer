using UnityEngine;
using UnityEngine.UI;

public class FriendsHUDController : IHUD
{
    public FriendsHUDView view
    {
        get;
        private set;
    }

    public FriendsHUDController()
    {
        view = FriendsHUDView.Create();
    }

    public void Initialize()
    {

    }

    public void Dispose()
    {
        if (view != null)
        {
            Object.Destroy(view.gameObject);
        }
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }
}
