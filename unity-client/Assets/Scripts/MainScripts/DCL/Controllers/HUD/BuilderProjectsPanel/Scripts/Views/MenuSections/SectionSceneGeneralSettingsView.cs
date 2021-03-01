using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SectionSceneGeneralSettingsView : MonoBehaviour
{
    [SerializeField] internal TMP_InputField nameInputField;
    [SerializeField] internal TMP_InputField descriptionInputField;
    [SerializeField] internal TMP_Text nameCharCount;
    [SerializeField] internal TMP_Text descriptionCharCount;
    [SerializeField] internal GameObject configurationContainer;
    [SerializeField] internal GameObject permissionsContainer;
    [SerializeField] internal Toggle toggleVoiceChat;
    [SerializeField] internal Toggle toggleEmotes;
    [SerializeField] internal Toggle toggleMatureContent;
    [SerializeField] internal Toggle toggleMovePlayer;
    [SerializeField] internal Button applyButton;

    public void SetName(string sceneName)
    {
        nameInputField.text = sceneName;
    }
    
    public void SetDescription(string sceneDescription)
    {
        descriptionInputField.text = sceneDescription;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetConfigurationActive(bool active)
    {
        configurationContainer.SetActive(active);
    }

    public void SetPermissionsActive(bool active)
    {
        permissionsContainer.SetActive(active);
    }

    public void SetAllowVoiceChat(bool allow)
    {
        toggleVoiceChat.isOn = allow;
    }

    public void SetAllowTriggerEmotes(bool allow)
    {
        toggleEmotes.isOn = allow;
    }

    public void SetAllowMovePlayer(bool allow)
    {
        toggleMovePlayer.isOn = allow;
    }

    public void SetMatureContent(bool mature)
    {
        toggleMatureContent.isOn = mature;
    }

    private void Awake()
    {
        nameInputField.onValueChanged.AddListener(value =>
        {
            nameCharCount.text = $"{value.Length}/{nameInputField.characterLimit}";
        });
        descriptionInputField.onValueChanged.AddListener(value =>
        {
            descriptionCharCount.text = $"{value.Length}/{descriptionInputField.characterLimit}";
        });        
    }
}
