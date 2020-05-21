using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the location information of a friend entry model.
/// It expose a button in order to be subscribed in any place.
/// To initialize this control, use UpdateInfo().
/// </summary>
public class JumpInButton : MonoBehaviour
{
    [SerializeField] internal Button button;
    [SerializeField] internal TextMeshProUGUI playerLocationText;

    private FriendEntryBase.Model currentModel;

    /// <summary>
    /// Updates the model information.
    /// </summary>
    /// <param name="model">A friend entry model</param>
    public void UpdateInfo(FriendEntryBase.Model model)
    {
        currentModel = model;
        ResfreshInfo();
    }

    private void ResfreshInfo()
    {
        if (currentModel.status == PresenceStatus.ONLINE)
        {
            playerLocationText.text = $"{currentModel.realm} {(int)currentModel.coords.x}, {(int)currentModel.coords.y}";
            button.gameObject.SetActive(true);
        }
        else
        {
            button.gameObject.SetActive(false);
            playerLocationText.text = string.Empty;
        }
    }
}
