using DCL.Interface;
using System;

public class WelcomeHUDController : IHUD, IDisposable
{
    //NOTE(Brian): This will be kept disabled until we get a way of getting the layout externally.
    public const bool ENABLE_DYNAMIC_CONTENT = false;

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

    WelcomeHUDView view;
    WelcomeHUDController.Model model;
    public WelcomeHUDController(Model model)
    {
        this.model = model;


        view = WelcomeHUDView.CreateView();

        if (ENABLE_DYNAMIC_CONTENT)
        {
            view.Initialize(model);

            if (model.showTime)
            {
                model.timeText += " 15 days"; //TODO(Brian): calculate time left
            }
        }

        view.confirmButton.onClick.RemoveAllListeners();
        view.confirmButton.onClick.AddListener(OnConfirmPressed);

        view.closeButton.onClick.RemoveAllListeners();
        view.closeButton.onClick.AddListener(Close);
    }

    void Close()
    {
        SetVisibility(false);
    }

    void OnConfirmPressed()
    {
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
