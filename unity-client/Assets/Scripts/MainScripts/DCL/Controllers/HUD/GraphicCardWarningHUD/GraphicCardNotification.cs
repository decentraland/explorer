using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

public class GraphicCardNotification : Notification
{
    [SerializeField] private string moreInfoUrl;
    [SerializeField] private Button moreInfoButton;

    private void Awake()
    {
        moreInfoButton.gameObject.SetActive(string.IsNullOrEmpty(moreInfoUrl));
        moreInfoButton.onClick.AddListener(OpenMoreInfoUrl);
    }

    private void OpenMoreInfoUrl()
    {
        WebInterface.OpenURL(moreInfoUrl);
    }
}
