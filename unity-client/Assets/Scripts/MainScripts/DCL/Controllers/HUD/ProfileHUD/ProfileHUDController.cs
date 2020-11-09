using DCL;
using UnityEngine;
using DCL.Interface;
using System.Collections;
using System;

public class ProfileHUDController : IHUD
{
    [Serializable]
    public struct Configuration
    {
        public bool connectedWallet;
    }

    private const string URL_CLAIM_NAME = "https://avatars.decentraland.org/claim";
    private const string URL_MANA_INFO = "https://docs.decentraland.org/examples/get-a-wallet";
    private const string URL_MANA_PURCHASE = "https://market.decentraland.org/settings";
    private const string URL_TERMS_OF_USE = "https://decentraland.org/terms";
    private const string URL_PRIVACY_POLICY = "https://decentraland.org/privacy";
    private const float FETCH_MANA_INTERVAL = 60;

    internal ProfileHUDView view;
    internal ManaCounterView manaCounterView;
    internal AvatarEditorHUDController avatarEditorHud;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private IMouseCatcher mouseCatcher;
    private Coroutine fetchManaIntervalRoutine = null;

    public RectTransform backpackTooltipReference { get => view.backpackTooltipReference; }

    public ProfileHUDController()
    {
        mouseCatcher = InitialSceneReferences.i?.mouseCatcher;

        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ProfileHUD")).GetComponent<ProfileHUDView>();
        view.name = "_ProfileHUD";

        SetBackpackButtonVisibility(false);
        view.connectedWalletSection.SetActive(false);
        view.nonConnectedWalletSection.SetActive(false);

        view.buttonBackpack.onClick.AddListener(OpenBackpackWindow);
        view.buttonLogOut.onClick.AddListener(WebInterface.LogOut);
        view.buttonSignUp.onClick.AddListener(WebInterface.RedirectToSignUp);
        view.buttonClaimName.onClick.AddListener(()=> WebInterface.OpenURL(URL_CLAIM_NAME));
        view.buttonTermsOfService.onPointerDown += () => WebInterface.OpenURL(URL_TERMS_OF_USE);
        view.buttonPrivacyPolicy.onPointerDown += () => WebInterface.OpenURL(URL_PRIVACY_POLICY);
        view.inputName.onSubmit.AddListener(UpdateProfileName);

        manaCounterView = view.GetComponentInChildren<ManaCounterView>(true);
        if (manaCounterView)
        {
            manaCounterView.buttonManaInfo.onPointerDown += () => WebInterface.OpenURL(URL_MANA_INFO);
            manaCounterView.buttonManaPurchase.onPointerDown += () => WebInterface.OpenURL(URL_MANA_PURCHASE);
        }

        ownUserProfile.OnUpdate += OnProfileUpdated;
        if (mouseCatcher != null) mouseCatcher.OnMouseLock += OnMouseLocked;
    }

    public void SetVisibility(bool visible)
    {
        view?.SetVisibility(visible);

        if (visible && fetchManaIntervalRoutine == null)
        {
            fetchManaIntervalRoutine = CoroutineStarter.Start(ManaIntervalRoutine());
        }
        else if (!visible && fetchManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchManaIntervalRoutine);
            fetchManaIntervalRoutine = null;
        }
    }

    public void Dispose()
    {
        if (fetchManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchManaIntervalRoutine);
            fetchManaIntervalRoutine = null;
        }

        if (view)
        {
            GameObject.Destroy(view.gameObject);
        }
        ownUserProfile.OnUpdate -= OnProfileUpdated;
        if (mouseCatcher != null) mouseCatcher.OnMouseLock -= OnMouseLocked;
    }

    void OnProfileUpdated(UserProfile profile)
    {
        view?.SetProfile(profile);
    }

    void OnMouseLocked()
    {
        HideProfileMenu();
    }

    IEnumerator ManaIntervalRoutine()
    {
        while (true)
        {
            WebInterface.FetchBalanceOfMANA();
            yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
        }
    }

    /// <summary>
    /// Set an amount of MANA on the HUD.
    /// </summary>
    /// <param name="balance">Amount of MANA.</param>
    public void SetManaBalance(string balance)
    {
        manaCounterView?.SetBalance(balance);
    }

    /// <summary>
    /// Configure an AvatarEditorHUDController for the Backpack button.
    /// </summary>
    /// <param name="controller">The avatar editor controller to asign.</param>
    public void AddBackpackWindow(AvatarEditorHUDController controller)
    {
        if (controller == null)
        {
            Debug.LogWarning("AddBackpackWindow >>> Backpack window doesn't exist yet!");
            return;
        }

        avatarEditorHud = controller;
        SetBackpackButtonVisibility(true);
    }

    /// <summary>
    /// Show/Hide the Backpack button.
    /// </summary>
    /// <param name="visible">True for showing the button.</param>
    public void SetBackpackButtonVisibility(bool visible)
    {
        view?.SetBackpackButtonVisibility(avatarEditorHud != null && visible);
    }

    private void OpenBackpackWindow()
    {
        if (avatarEditorHud == null)
            return;

        avatarEditorHud.SetVisibility(true);
        HideProfileMenu();
    }

    /// <summary>
    /// Close the Profile menu.
    /// </summary>
    public void HideProfileMenu()
    {
        view?.HideMenu();
    }

    private void UpdateProfileName(string newName)
    {
        if (view != null)
        {
            view.SetProfileName(newName);
            view.ActivateProfileNameEditionMode(false);
        }

        WebInterface.SendSaveUserUnverifiedName(newName);
    }
}
