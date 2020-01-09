using System;
using DCL.Interface;

public class ExpressionsHUDController : IHUD, IDisposable
{
    internal ExpressionsHUDView view;

    public ExpressionsHUDController()
    {
        view = ExpressionsHUDView.Create();
        view.Initialize(ExpressionCalled);
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisiblity(visible);
    }

    public void Dispose()
    {
        view.CleanUp();
    }

    internal void ExpressionCalled(string id)
    {
        var timestamp = DateTime.UtcNow.Ticks;
        UserProfile.GetOwnUserProfile().SetAvatarExpression(id, timestamp);
        WebInterface.SendExpression(id, timestamp);
    }
}