using UnityEngine;
using DCL.Interface;

public class ProfileHUDController : IHUD
{
    private const string URL_CLAIM_NAME = "https://avatars.decentraland.org/claim";

    internal ProfileHUDView view;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    public bool talking { get; private set; }

    public ProfileHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ProfileHUD")).GetComponent<ProfileHUDView>();
        view.name = "_ProfileHUD";

        view.buttonLogOut.onClick.AddListener(WebInterface.LogOut);
        view.buttonClaimName.onClick.AddListener(()=> WebInterface.OpenURL(URL_CLAIM_NAME));

        ownUserProfile.OnUpdate += OnProfileUpdated;
    }

    public void SetVisibility(bool visible)
    {
        view?.gameObject.SetActive(visible);
    }

    public void SetTalking(bool value)
    {
        talking = value;
        view.SetTalking(talking);
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
