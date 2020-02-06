using DCL.Interface;
using System;

public class WelcomeHUDController : IHUD, IDisposable
{
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

        if (model.showTime)
        {
            model.timeText += " 15 days"; //TODO(Brian): calculate time left
        }

        view = WelcomeHUDView.CreateView();
        view.Initialize(model);

        view.confirmButton.onClick.RemoveAllListeners();
        view.confirmButton.onClick.AddListener(OnConfirmPressed);
    }

    void OnConfirmPressed()
    {
        WebInterface.SendChatCommand(model.buttonAction);
    }

    public void Dispose()
    {
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }
}
