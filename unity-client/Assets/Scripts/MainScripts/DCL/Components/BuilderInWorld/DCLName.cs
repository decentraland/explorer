using DCL;
using DCL.Components;
using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCLName : BaseDisposable
{
    [System.Serializable]
    public class Model
    {
        public string value;
    }

    public Model model;

    public DCLName(ParcelScene scene) : base(scene)
    {
        model = new Model();
    }

    public override IEnumerator ApplyChanges(string newJson)
    {
        Model newModel = SceneController.i.SafeFromJson<Model>(newJson);
        if(newModel.value != model.value)
        {
            model = newModel;
            RaiseOnAppliedChanges();
        }
    
        yield return null;
    }
}
