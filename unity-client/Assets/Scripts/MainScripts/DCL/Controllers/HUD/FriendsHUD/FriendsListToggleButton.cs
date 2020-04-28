using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FriendsListToggleButton : MonoBehaviour
{
    public Button toggleButton;
    public Transform toggleButtonIcon;
    public GameObject container;

    void Awake()
    {
        toggleButton.onClick.AddListener(() =>
        {
            container.SetActive(!container.activeSelf);
            toggleButtonIcon.localScale = new Vector3(toggleButtonIcon.localScale.x, -toggleButtonIcon.localScale.y, 1f);
        });
    }
}
