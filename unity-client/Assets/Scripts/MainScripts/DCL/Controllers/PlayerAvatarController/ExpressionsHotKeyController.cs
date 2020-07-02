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
    [SerializeField] private ExpressionInputActionToId raiseHandExpression;
    [SerializeField] private ExpressionInputActionToId clapExpression;
    [SerializeField] private ExpressionInputActionToId throwMoneyExpression;
    [SerializeField] private ExpressionInputActionToId sendKissExpression;

    private InputAction_Trigger.Triggered waveDelegate;
    private InputAction_Trigger.Triggered robotDelegate;
    private InputAction_Trigger.Triggered fistpumpDelegate;
    private InputAction_Trigger.Triggered raiseHandDelegate;
    private InputAction_Trigger.Triggered clapDelegate;
    private InputAction_Trigger.Triggered throwMoneyDelegate;
    private InputAction_Trigger.Triggered sendKissDelegate;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private bool usingMaleAnimations = true;
    private bool alreadySetAnimations = false;

    private void Awake()
    {
        ownUserProfile.OnUpdate += UpdateAnimationsIDBasedOnSex;

        waveDelegate = (x) => ExpressionCalled(waveExpression.id);
        waveExpression.inputAction.OnTriggered += waveDelegate;

        robotDelegate = (x) => ExpressionCalled(robotExpression.id);
        robotExpression.inputAction.OnTriggered += robotDelegate;

        fistpumpDelegate = (x) => ExpressionCalled(fistpumpExpression.id);
        fistpumpExpression.inputAction.OnTriggered += fistpumpDelegate;

        raiseHandDelegate = (x) => ExpressionCalled(raiseHandExpression.id);
        raiseHandExpression.inputAction.OnTriggered += raiseHandDelegate;

        clapDelegate = (x) => ExpressionCalled(clapExpression.id);
        clapExpression.inputAction.OnTriggered += clapDelegate;

        throwMoneyDelegate = (x) => ExpressionCalled(throwMoneyExpression.id);
        throwMoneyExpression.inputAction.OnTriggered += throwMoneyDelegate;

        sendKissDelegate = (x) => ExpressionCalled(sendKissExpression.id);
        sendKissExpression.inputAction.OnTriggered += sendKissDelegate;
    }

    private void ExpressionCalled(string id)
    {
        ownUserProfile.SetAvatarExpression(id);
    }

    private void UpdateAnimationsIDBasedOnSex(UserProfile userProfile)
    {
        if (alreadySetAnimations && userProfile.isMale == usingMaleAnimations) return;

        usingMaleAnimations = userProfile.isMale;

        waveExpression.id = (usingMaleAnimations ? "M_" : "F_") + waveExpression.id;
        robotExpression.id = (usingMaleAnimations ? "M_" : "F_") + robotExpression.id;
        fistpumpExpression.id = (usingMaleAnimations ? "M_" : "F_") + fistpumpExpression.id;
        raiseHandExpression.id = (usingMaleAnimations ? "M_" : "F_") + raiseHandExpression.id;
        clapExpression.id = (usingMaleAnimations ? "M_" : "F_") + clapExpression.id;
        throwMoneyExpression.id = (usingMaleAnimations ? "M_" : "F_") + throwMoneyExpression.id;
        sendKissExpression.id = (usingMaleAnimations ? "M_" : "F_") + sendKissExpression.id;

        alreadySetAnimations = true;
    }
}