using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeEntityAction 
{
    public DecentralandEntity entity;
    public object oldValue, newValue;


    public BuildModeEntityAction(DecentralandEntity _entity)
    {
        entity = _entity;
    }
    public BuildModeEntityAction(DecentralandEntity _entity,object _oldValue,object _newValue)
    {
        entity = _entity;
        oldValue = _oldValue;
        newValue = _newValue;
    }
}
