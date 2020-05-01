using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FriendsListToggleButton : MonoBehaviour
{
    public Button toggleButton;
    public Transform toggleButtonIcon;
    public RectTransform containerRectTransform;

    void Awake()
    {
        toggleButton.onClick.AddListener(() =>
        {
            containerRectTransform.gameObject.SetActive(!containerRectTransform.gameObject.activeSelf);
            toggleButtonIcon.localScale = new Vector3(toggleButtonIcon.localScale.x, -toggleButtonIcon.localScale.y, 1f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRectTransform);
        });
    }
}
