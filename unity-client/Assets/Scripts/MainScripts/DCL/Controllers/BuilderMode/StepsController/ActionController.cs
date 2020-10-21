using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    public ActionListview actionListview;
    public System.Action OnUndo, OnRedo;
    List<BuildModeAction> actionsMade = new List<BuildModeAction>();


    private void Awake()
    {
        actionListview.OnActionSelected += GoToAction;
    }

    int currentStepIndex = 0;
    public void ClearActionList()
    {
        actionsMade.Clear();
    }

    public void GoToAction(BuildModeAction action)
    {
        int index = actionsMade.IndexOf(action);
        int stepsAmount = currentStepIndex - index;

        for(int i = 0; i <= Mathf.Abs(stepsAmount); i++)
        {
            if (stepsAmount > 0)
            {
                UndoCurrentAction();
                if (currentStepIndex > 0) currentStepIndex--;
            }
            else
            {
                RedoCurrentAction();
                if (currentStepIndex + 1 < actionsMade.Count) currentStepIndex++;
            }
        }

    }




    public void TryToRedoAction()
    {
        if (currentStepIndex < actionsMade.Count)
        {
            if (currentStepIndex + 1 < actionsMade.Count)
            {
                if(currentStepIndex == 0 && actionsMade[currentStepIndex].isDone) currentStepIndex++;
                else if(currentStepIndex != 0) currentStepIndex++;
                RedoCurrentAction();
            }
        }
    }

    public void TryToUndoAction()
    {
        if (currentStepIndex >= 0 && actionsMade.Count > 0)
        {

            UndoCurrentAction();
            if (currentStepIndex > 0) currentStepIndex--;
        }
    }

    public void AddAction(BuildModeAction action)
    {
        bool removedActions = false;
        if (currentStepIndex < actionsMade.Count-1)
        {
            actionsMade.RemoveRange(currentStepIndex, actionsMade.Count - currentStepIndex);
            removedActions = true;
        }
        actionsMade.Add(action);
    
        currentStepIndex = actionsMade.Count-1;
        if (removedActions) actionListview.SetContent(actionsMade);
        else actionListview.AddAdapter(action);
    }

    void RedoCurrentAction()
    {
        if (!actionsMade[currentStepIndex].isDone)
        {
            actionsMade[currentStepIndex].ReDo();
            actionListview.RefreshInfo();
            OnRedo?.Invoke();
          
        }
  
    }

    void UndoCurrentAction()
    {
        if (actionsMade[currentStepIndex].isDone)
        {
            actionsMade[currentStepIndex].Undo();
            actionListview.RefreshInfo();
            OnUndo?.Invoke();         
        }
    }


}
