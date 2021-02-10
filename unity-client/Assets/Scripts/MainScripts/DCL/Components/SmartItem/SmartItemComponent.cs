using System.Collections;
using DCL.Controllers;
using DCL.Helpers;

namespace DCL.Components
{
    public class SmartItemComponent : BaseComponent
    {
        public class Model
        {
            public SmartItemAction[] actions;
            public SmartItemParameter[] parameters;
            public SmartItemValues values;
        }

        public Model model;

        public override object GetModel()
        {
            return model;
        }

        public bool HasActions()
        {
            if (model.actions.Length > 0)
                return true;

            return false;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            Model newModel = Utils.SafeFromJson<Model>(newJson);
            model = newModel;
            yield break;
        }

        public override void SetModel(object model)
        {
            this.model = (Model)model;
        }

    }
}