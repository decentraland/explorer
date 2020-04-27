using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendsHUDView : MonoBehaviour
{
    const string VIEW_PATH = "FriendsHUD";

    public Button closeButton;

    public static FriendsHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<FriendsHUDView>();
        view.Initialize();
        return view;
    }

    private void Initialize()
    {
        closeButton.onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
