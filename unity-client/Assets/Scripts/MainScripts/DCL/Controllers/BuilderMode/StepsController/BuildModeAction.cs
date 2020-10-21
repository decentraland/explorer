using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeAction
{
    public enum ActionType
    {
        MOVE = 0,
        ROTATE = 1,
        SCALE = 2,
        CREATED = 3
    }
    public ActionType actionType;
 
    List<BuildModeEntityAction> entitiyApplied = new List<BuildModeEntityAction>();

    //object fromValue,toValue;
    public bool isDone = true;
    
    public void ReDo()
    {
        foreach(BuildModeEntityAction action in entitiyApplied)
        {
            ApplyValue(action.entity,action.newValue);
        }
        isDone = true;
     
    }
    public void Undo()
    {
        foreach (BuildModeEntityAction action in entitiyApplied)
        {
            ApplyValue(action.entity, action.oldValue);
        }

        isDone = false;

    }

    void ApplyValue(DecentralandEntity entityToApply, object value)
    {
        switch (actionType)
        {
            case ActionType.MOVE:
                Vector3 convertedPosition = (Vector3)value;
                entityToApply.gameObject.transform.position = convertedPosition;
                break;
            case ActionType.ROTATE:
                Vector3 convertedAngles = (Vector3)value;
                entityToApply.gameObject.transform.eulerAngles = convertedAngles;
                break;
            case ActionType.SCALE:
                Vector3 convertedScale = (Vector3)value;
                Transform parent = entityToApply.gameObject.transform.parent;

                entityToApply.gameObject.transform.localScale = new Vector3( convertedScale.x / parent.localScale.x ,  convertedScale.y / parent.localScale.y ,convertedScale.z  / parent.localScale.z );
                //entityToApply.gameObject.transform.localScale = convertedScale;
                break;
            case ActionType.CREATED:
                break;

        }
    }

    public void CreateActionType(List<BuildModeEntityAction> entitiesActions, ActionType type)
    {
        CreateAction(entitiesActions, type);
    }
    public void CreateActionTypeMove(List<BuildModeEntityAction> entitiesActions)
    {
        CreateAction(entitiesActions, ActionType.MOVE);
    }
    public void CreateActionTypeRotate(List<BuildModeEntityAction> entitiesActions)
    {
        CreateAction(entitiesActions, ActionType.ROTATE);
    }
    public void CreateActionTypeScale(List<BuildModeEntityAction> entitiesActions)
    {
        CreateAction(entitiesActions, ActionType.SCALE);
    }


    void CreateAction(List<BuildModeEntityAction> entitiesActions,ActionType type)
    {
        actionType = type;
        entitiyApplied = entitiesActions;
        isDone = true;
    }
}
