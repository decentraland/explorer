using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldChatWindowHUDView : MonoBehaviour, IPointerClickHandler
{
    const string VIEW_PATH = "World Chat Window";

    public Button worldFilterButton;
    public Button pmFilterButton;
    public Button closeButton;

    public ChatHUDView chatHudView;

    public CanvasGroup group;
    public WorldChatWindowHUDController controller;

    public static WorldChatWindowHUDView Create(UnityAction onPrivateMessages, UnityAction onWorldMessages)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<WorldChatWindowHUDView>();
        view.Initialize(onPrivateMessages, onWorldMessages);
        return view;
    }

    private void Initialize(UnityAction onPrivateMessages, UnityAction onWorldMessages)
    {
        this.closeButton.onClick.AddListener(Toggle);
        this.pmFilterButton.onClick.AddListener(onPrivateMessages);
        this.worldFilterButton.onClick.AddListener(onWorldMessages);
    }

    public bool isInPreview { get; private set; }

    public void DeactivatePreview()
    {
        group.alpha = 1;
        isInPreview = false;
    }

    public void ActivatePreview()
    {
        group.alpha = 0;
        isInPreview = true;
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        else
        {
            gameObject.SetActive(true);
            DeactivatePreview();
            chatHudView.ForceUpdateLayout();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DeactivatePreview();
    }
}
