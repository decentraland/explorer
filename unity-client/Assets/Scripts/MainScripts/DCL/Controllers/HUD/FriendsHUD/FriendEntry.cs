using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendEntry : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI playerNameText;
    [SerializeField] internal TextMeshProUGUI playerLocationText;
    [SerializeField] internal Image playerImage;
    [SerializeField] internal Button jumpInButton;
    [SerializeField] internal Button whisperButton;

    public struct Model
    {
        public enum Status
        {
            NONE,
            OFFLINE,
            ONLINE,
            LOADING,
            AFK
        }

        public Status status;
        public string userName;
        public Vector2 coords;
        public string realm;
        public Sprite avatarImage;
    }

    public void Populate(Model model)
    {
        playerNameText.text = model.userName;
        playerLocationText.text = $"{model.realm} {model.coords}";

        playerImage.sprite = model.avatarImage;

        jumpInButton.onClick.RemoveAllListeners();
        jumpInButton.onClick.AddListener(() => WebInterface.GoTo((int)model.coords.x, (int)model.coords.y));

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(WhisperPlayer);
    }

    void WhisperPlayer()
    {
        // TODO: trigger private message to the user
    }
}
