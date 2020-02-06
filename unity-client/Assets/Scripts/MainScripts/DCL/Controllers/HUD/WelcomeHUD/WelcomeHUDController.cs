using DCL.Helpers;
using DCL.Interface;
using System;

public class WelcomeHUDController : IHUD, IDisposable
{
    //NOTE(Brian): This will be kept disabled until we get a way of getting the layout externally.
    public static bool ENABLE_DYNAMIC_CONTENT = false;

    [System.Serializable]
    public class Model : HUDConfiguration
    {
        public string title;

        public ulong timeTarget;
        public string timeText;
        public bool showTime;

        public string bodyText;

        public string buttonText;
        public string buttonAction;

        public bool showButton;
    }

    internal WelcomeHUDView view;
    internal WelcomeHUDController.Model model;

    public void Initialize(Model model)
    {
        this.model = model;

        if (ENABLE_DYNAMIC_CONTENT)
        {
            view.Initialize(model);
        }
    }

    public WelcomeHUDController()
    {
        view = WelcomeHUDView.CreateView();

        view.confirmButton.onClick.RemoveAllListeners();
        view.confirmButton.onClick.AddListener(OnConfirmPressed);

        view.closeButton.onClick.RemoveAllListeners();
        view.closeButton.onClick.AddListener(Close);

        Utils.UnlockCursor();
    }

    internal void Close()
    {
        SetVisibility(false);
        Utils.LockCursor();
    }

    void OnConfirmPressed()
    {
        if (model != null && !string.IsNullOrEmpty(model.buttonAction))
            WebInterface.SendChatCommand(model.buttonAction);

        Close();
    }

    public void Dispose()
    {
        view.confirmButton.onClick.RemoveAllListeners();
        view.closeButton.onClick.RemoveAllListeners();
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }
}
