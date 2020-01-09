using System;
using UnityEngine;
using UnityEngine.UI;

public class ExpressionsHUDView : MonoBehaviour
{
    private const string PATH = "ExpressionsHUD";

    public delegate void ExpressionClicked(string expressionId);

    [Serializable]
    public class ButtonToExpression
    {
        public string expressionId;
        public Button button;
    }

    [SerializeField] private ButtonToExpression[] buttonToExpressionMap;
    [SerializeField] private Button showContentButton;
    [SerializeField] private Button hideContentButton;
    [SerializeField] private RectTransform content;
    [SerializeField] private InputAction_Trigger openExpressionsAction;
    private InputAction_Trigger.Triggered openExpressionsDelegate;

    public static ExpressionsHUDView Create()
    {
        return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<ExpressionsHUDView>();
    }

    private void Awake()
    {
        openExpressionsDelegate = (x) => {ToggleContent();};
        openExpressionsAction.OnTriggered += openExpressionsDelegate;
        hideContentButton.onClick.AddListener(HideContent);
        showContentButton.onClick.AddListener(ToggleContent);
    }

    internal void Initialize(ExpressionClicked clickedDelegate)
    {
        HideContent();

        foreach (var buttonToExpression in buttonToExpressionMap)
        {
            buttonToExpression.button.onClick.RemoveAllListeners();
            buttonToExpression.button.onClick.AddListener(() =>
                {
                    clickedDelegate.Invoke(buttonToExpression.expressionId);
                    HideContent();
                }
            );
        }
    }

    private void ToggleContent()
    {
        if (content.gameObject.activeSelf)
        {
            HideContent();
        }
        else
        {
            ShowContent();
        }
    }

    private void ShowContent()
    {
        content.gameObject.SetActive(true);
        DCL.Helpers.Utils.UnlockCursor();
    }

    private void HideContent()
    {
        content.gameObject.SetActive(false);
        DCL.Helpers.Utils.LockCursor();
    }

    public void SetVisiblity(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void CleanUp()
    {
        openExpressionsAction.OnTriggered -= openExpressionsDelegate;
    }
}
