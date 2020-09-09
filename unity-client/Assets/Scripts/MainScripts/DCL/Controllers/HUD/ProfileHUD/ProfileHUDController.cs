using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileHUDController : IHUD
{
    internal readonly ProfileHUDView view;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    public ProfileHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ProfileHUD")).GetComponent<ProfileHUDView>();
        view.name = "_ProfileHUD";

        ownUserProfile.OnUpdate += OnProfileUpdated;
    }

    public void SetVisibility(bool visible)
    {
        view?.gameObject.SetActive(visible);
    }

    public void Dispose()
    {
        if (view)
        {
            Object.Destroy(view.gameObject);
        }
        ownUserProfile.OnUpdate -= OnProfileUpdated;
    }

    void OnProfileUpdated(UserProfile profile)
    {
        view?.SetProfile(profile);
    }
}
