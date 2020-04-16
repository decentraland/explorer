using System;

public class ChatTaskbarHUDController : IDisposable, IHUD
{
    ChatTaskbarHUDView view;

    public ChatTaskbarHUDController()
    {
        view = ChatTaskbarHUDView.Create(this);
    }
    public void Dispose()
    {
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }
}
