using UnityEngine;
using DCL.Interface;
using DCL.Helpers;
using System;
using System.Collections;
using DCL.Tutorial;

public class EmailPromptHUDController : IHUD
{
    EmailPromptHUDView view;

    bool isRendererEnabled = false;
    bool isTimerComplete = false;
    bool isTimerCreated = false;
    bool isEnable = false;

    Coroutine timerRoutine;

    public EmailPromptHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("EmailPromptHUD")).GetComponent<EmailPromptHUDView>();
        view.name = "_EmailPromptHUD";

        view.OnDismiss += OnDismiss;
        view.OnSendEmail += OnSendEmail;

        view.gameObject.SetActive(false);

        CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            Utils.UnlockCursor();
            view.gameObject.SetActive(true);
            view.showHideAnimator.Show();
        }
        else
        {
            view.showHideAnimator.Hide();
        }
    }

    public void Dispose()
    {
        CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;

        if (view != null)
        {
            GameObject.Destroy(view.gameObject);
        }
        if (timerRoutine != null)
        {
            CoroutineStarter.Stop(timerRoutine);
        }
    }

    public void SetEnable(bool enable)
    {
        bool wasEnabled = isEnable;
        isEnable = enable;

        if (enable && !wasEnabled)
        {
            if (CommonScriptableObjects.rendererState)
            {
                isRendererEnabled = true;
                CreateTimer();
            }
        }
        else if (!enable && wasEnabled)
        {
            if (timerRoutine != null)
            {
                CoroutineStarter.Stop(timerRoutine);
            }
            isTimerComplete = false;
            isTimerCreated = false;
        }
    }

    void CreateTimer()
    {
        if (!isEnable)
            return;

        isTimerCreated = true;
        timerRoutine = CoroutineStarter.Start(WaitForSeconds(60));
    }

    IEnumerator WaitForSeconds(float seconds)
    {
        yield return WaitForSecondsCache.Get(seconds);
        isTimerComplete = true;
        CheckStateToDisplay();
    }

    void CheckStateToDisplay()
    {
        if (isTimerComplete && isRendererEnabled && isEnable)
        {
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            SetVisibility(true);
        }
    }

    void OnRendererStateChanged(bool prev, bool current)
    {
        isRendererEnabled = true;
        if (!isTimerCreated)
        {
            CreateTimer();
        }
        else
        {
            CheckStateToDisplay();
        }
    }

    void OnSendEmail(string email)
    {
        WebInterface.SendUserEmail(email);
        SetEmailFlag();
        SetVisibility(false);
    }

    void OnDismiss(bool dontAskAgain)
    {
        if (dontAskAgain)
        {
            SetEmailFlag();
        }
        SetVisibility(false);
    }

    void SetEmailFlag()
    {
        TutorialController.i.SetStepCompleted(TutorialController.TutorialStep.EmailRequested);
    }
}
