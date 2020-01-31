using System;
using UnityEngine;

public class ExpressionsHotKeyController : MonoBehaviour
{
    [Serializable]
    private class ExpressionInputActionToId
    {
        public InputAction_Trigger inputAction;
        public string id;
    }

    [SerializeField] private ExpressionInputActionToId waveExpression;
    [SerializeField] private ExpressionInputActionToId robotExpression;
    [SerializeField] private ExpressionInputActionToId fistpumpExpression;

    private InputAction_Trigger.Triggered waveDelegate;
    private InputAction_Trigger.Triggered robotDelegate;
    private InputAction_Trigger.Triggered fistpumpDelegate;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    private void Awake()
    {
        waveDelegate = (x) => ExpressionCalled(waveExpression.id);
        waveExpression.inputAction.OnTriggered += waveDelegate;

        robotDelegate = (x) => ExpressionCalled(robotExpression.id);
        robotExpression.inputAction.OnTriggered += robotDelegate;

        fistpumpDelegate = (x) => ExpressionCalled(fistpumpExpression.id);
        fistpumpExpression.inputAction.OnTriggered += fistpumpDelegate;
    }

    private void ExpressionCalled(string id)
    {
        return;
        ownUserProfile.SetAvatarExpression(id);
    }
}