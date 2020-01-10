using System;
using DCL.Interface;

public class ExpressionsHUDController : IHUD, IDisposable
{
    internal ExpressionsHUDView view;
    static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
        var timestamp = (long)(DateTime.UtcNow - epochStart).TotalMilliseconds;
        UserProfile.GetOwnUserProfile().SetAvatarExpression(id, timestamp);
        WebInterface.SendExpression(id, timestamp);
    }
}