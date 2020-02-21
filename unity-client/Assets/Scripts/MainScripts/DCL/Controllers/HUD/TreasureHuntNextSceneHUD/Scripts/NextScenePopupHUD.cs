using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;

public class NextScenePopupHUD : MonoBehaviour
{
    [SerializeField] GameObject popupContainer;
    [SerializeField] Button closeButton;
    [SerializeField] Button gotoNextButton;

    void Awake()
    {
        closeButton.onClick.AddListener(Close);
        gotoNextButton.onClick.AddListener(GotToNextScene);
        popupContainer.SetActive(false);
    }

    void Close()
    {
        popupContainer.SetActive(false);
    }

    void GotToNextScene()
    {
        WebInterface.GoToNextTreasureHuntScene();
        Close();
    }

    public void Show()
    {
        popupContainer.SetActive(true);
    }
}
