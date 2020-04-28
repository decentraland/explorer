using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;

public class FriendEntry : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI playerNameText;
    [SerializeField] internal TextMeshProUGUI playerLocationText;
    [SerializeField] internal Image playerImage;
    [SerializeField] internal Button jumpInButton;
    [SerializeField] internal Button whisperButton;

    public void Populate(string playerName, Vector2 playerCoords, string playerRealm, Sprite playerAvatarImage)
    {
        playerNameText.text = playerName;
        playerLocationText.text = $"{playerRealm} {playerCoords}";

        playerImage.sprite = playerAvatarImage;

        jumpInButton.onClick.RemoveAllListeners();
        jumpInButton.onClick.AddListener(() => WebInterface.GoTo((int)playerCoords.x, (int)playerCoords.y));

        whisperButton.onClick.RemoveAllListeners();
        // whisperButton.onClick.AddListener(() => );
    }
}
