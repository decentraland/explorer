using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;

namespace DCL.Components
{
    public class SmartItemComponent : BaseComponent
    {
        public class Model : BaseModel
        {
            public Dictionary<object, object> values;

            public override bool Equals(object obj)
            {
                return obj is Model model &&
                       EqualityComparer<Dictionary<object, object>>.Default.Equals(values, model.values);
            }

            public override BaseModel GetDataFromJSON(string json)
            {
                return JsonConvert.DeserializeObject<Model>(json);
            }

            public override int GetHashCode()
            {
                return 1649527923 + EqualityComparer<Dictionary<object, object>>.Default.GetHashCode(values);
            }
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            yield break;
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.SMART_ITEM;
        }

        public Dictionary<object, object> GetValues()
        {
            return ((Model)model).values;
        }
    }
}