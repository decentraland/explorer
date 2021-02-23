using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Models;

namespace DCL.Components
{
    public class DCLGizmos : BaseComponent
    {
        public static class Gizmo
        {
            public const string MOVE = "MOVE";
            public const string ROTATE = "ROTATE";
            public const string SCALE = "SCALE";
            public const string NONE = "NONE";
        }

        [System.Serializable]
        public class Model : BaseModel
        {
            public bool position = true;
            public bool rotation = true;
            public bool scale = true;
            public bool cycle = true;
            public string selectedGizmo = Gizmo.NONE;
            public bool localReference = false;

            public override bool Equals(object obj)
            {
                return obj is Model model &&
                       position == model.position &&
                       rotation == model.rotation &&
                       scale == model.scale &&
                       cycle == model.cycle &&
                       selectedGizmo == model.selectedGizmo &&
                       localReference == model.localReference;
            }

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            public override int GetHashCode()
            {
                int hashCode = -1948302438;
                hashCode = hashCode * -1521134295 + position.GetHashCode();
                hashCode = hashCode * -1521134295 + rotation.GetHashCode();
                hashCode = hashCode * -1521134295 + scale.GetHashCode();
                hashCode = hashCode * -1521134295 + cycle.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(selectedGizmo);
                hashCode = hashCode * -1521134295 + localReference.GetHashCode();
                return hashCode;
            }
        }

        public override IEnumerator ApplyChanges(BaseModel baseModel)
        {
            yield return null;
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.GIZMOS;
        }
    }
}