using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// It represents a Portable Experience item in the taskbar.
/// </summary>
public class PortableExperienceTaskbarItem : MonoBehaviour
{
    [SerializeField]
    private TaskbarButton button;

    [SerializeField]
    private TextMeshProUGUI tooltipText;

    [SerializeField]
    private CanvasGroup tooltipTextContainerCanasGroup;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private PortableExperienceContextMenu contextMenu;

    public TaskbarButton mainButton { get => button; }

    internal void ConfigureItem(
        string portableExperienceId,
        string portableExperienceName,
        string portableExperienceIconUrl,
        TaskbarHUDController taskbarController)
    {
        tooltipText.text = portableExperienceName;
        button.Initialize();
        contextMenu.Initialize(portableExperienceId, taskbarController);

        if (!string.IsNullOrEmpty(portableExperienceIconUrl))
        {
            ThumbnailsManager.GetThumbnail(portableExperienceIconUrl, OnIconReady);
        }
    }

    private void OnIconReady(Asset_Texture iconAsset)
    {
        if (iconAsset != null)
        {
            icon.sprite = ThumbnailsManager.CreateSpriteFromTexture(iconAsset.texture);
        }
    }

    internal void ShowContextMenu(bool visible)
    {
        tooltipTextContainerCanasGroup.alpha = visible ? 0f : 1f;
        contextMenu.ShowMenu(visible);
    }
}
