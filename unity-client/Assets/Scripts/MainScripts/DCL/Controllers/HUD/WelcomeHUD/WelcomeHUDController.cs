using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

public class WelcomeHUDController : IHUD, System.IDisposable
{
    [System.Serializable]
    public class Model : HUDConfiguration
    {
        public bool hasWallet;
        public string buttonCommand;
    }

    internal WelcomeHUDView view;
    internal Model model;

    public void Initialize(Model model)
    {
        this.model = model;

        view = WelcomeHUDView.CreateView(model.hasWallet);
        view.Initialize(model, OnConfirmPressed, Close);

        Utils.UnlockCursor();
    }

    internal void Close()
    {
        SetVisibility(false);
        Utils.LockCursor();
    }

    void OnConfirmPressed()
    {
        if (model != null && !string.IsNullOrEmpty(model.buttonCommand))
            WebInterface.SendMotdClick(model.buttonCommand);

        Close();
    }

    public void Dispose()
    {
        Object.Destroy(view.gameObject);
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }
}
