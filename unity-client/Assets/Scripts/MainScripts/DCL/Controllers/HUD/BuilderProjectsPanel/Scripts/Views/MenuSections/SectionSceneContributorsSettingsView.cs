using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

internal class SectionSceneContributorsSettingsView : MonoBehaviour
{
    [SerializeField] internal UsersSearchPromptView usersSearchPromptView;
    [SerializeField] internal Button addUserButton;

    public event Action OnAddUserPressed;

    private void Awake()
    {
        addUserButton.onClick.AddListener(()=> OnAddUserPressed?.Invoke());
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

    public UsersSearchPromptView GetSearchPromptView()
    {
        return usersSearchPromptView;
    }
}
