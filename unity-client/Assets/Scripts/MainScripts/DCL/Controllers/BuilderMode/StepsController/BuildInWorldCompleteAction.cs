using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildInWorldCompleteAction
{
    public enum ActionType
    {
        MOVE = 0,
        ROTATE = 1,
        SCALE = 2,
        CREATED = 3,
        DELETED = 4
    }

    public ActionType actionType;
    public bool isDone = true;
    public System.Action<string, object, ActionType, bool> OnApplyValue;
    List<BuilderInWorldEntityAction> entitiyApplied = new List<BuilderInWorldEntityAction>();
    
    public void ReDo()
    {
        foreach(BuilderInWorldEntityAction action in entitiyApplied)
        {
            ApplyValue(action.entityId,action.newValue, false);
        }
        isDone = true;
    }

    public void Undo()
    {
        foreach (BuilderInWorldEntityAction action in entitiyApplied)
        {
            ApplyValue(action.entityId, action.oldValue, true);
        }

        isDone = false;

    }

    void ApplyValue(string entityToApply, object value, bool isUndo)
    {
        OnApplyValue?.Invoke(entityToApply, value, actionType, isUndo);
    }

    public void CreateActionType(BuilderInWorldEntityAction action, ActionType type)
    {
        List<BuilderInWorldEntityAction> list = new List<BuilderInWorldEntityAction>();
        list.Add(action);
        CreateAction(list, type);
    }

    public void CreateActionType(List<BuilderInWorldEntityAction> entitiesActions, ActionType type)
    {
        CreateAction(entitiesActions, type);
    }

    void CreateAction(List<BuilderInWorldEntityAction> entitiesActions,ActionType type)
    {
        actionType = type;
        entitiyApplied = entitiesActions;
        isDone = true;
    }
}
