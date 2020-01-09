using System;
using UnityEngine;

public class ExpressionsHUDController : IHUD, IDisposable
{
    private const string CURRENT_PLAYER_EXPRESSION_PATH = "OwnPlayerCurrentExpression";

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
        UserProfile.GetOwnUserProfile().SetAvatarExpression(id);
        //TODO Report to kernel
    }
}