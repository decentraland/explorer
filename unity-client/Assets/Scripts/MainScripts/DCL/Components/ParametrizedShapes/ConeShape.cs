using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using UnityEngine;

namespace DCL.Components
{
    public class ConeShape : ParametrizedShape<ConeShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
            public float radiusTop = 0f;
            public float radiusBottom = 1f;
            public float segmentsHeight = 1f;
            public float segmentsRadial = 36f;
            public bool openEnded = false;
            public float? radius;
            public float arc = 360f;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            protected bool Equals(Model other)
            {
                return base.Equals(other) && radiusTop.Equals(other.radiusTop) && radiusBottom.Equals(other.radiusBottom) &&
                       segmentsHeight.Equals(other.segmentsHeight) && segmentsRadial.Equals(other.segmentsRadial) &&
                       openEnded == other.openEnded && Nullable.Equals(radius, other.radius) && arc.Equals(other.arc) &&
                       withCollisions == other.withCollisions && isPointerBlocker == other.isPointerBlocker && visible == other.visible;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                if (obj.GetType() != this.GetType())
                    return false;

                return Equals((Model)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = radiusTop.GetHashCode();
                    hashCode = (hashCode * 397) ^ radiusBottom.GetHashCode();
                    hashCode = (hashCode * 397) ^ segmentsHeight.GetHashCode();
                    hashCode = (hashCode * 397) ^ segmentsRadial.GetHashCode();
                    hashCode = (hashCode * 397) ^ openEnded.GetHashCode();
                    hashCode = (hashCode * 397) ^ radius.GetHashCode();
                    hashCode = (hashCode * 397) ^ arc.GetHashCode();
                    return hashCode;
                }
            }
        }
        
        public ConeShape(IParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.CONE_SHAPE;
        }

        public override Mesh GenerateGeometry()
        {
            var model = (Model)this.model;
            return PrimitiveMeshBuilder.BuildCone(50, model.radiusTop, model.radiusBottom, 2f, 0f, true, false);
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            if (currentMesh == null) return true;

            Model newConeModel = newModel as Model;
            var model = (Model)this.model;
            return newConeModel.radius != model.radius
                    || newConeModel.radiusTop != model.radiusTop
                    || newConeModel.radiusBottom != model.radiusBottom
                    || newConeModel.segmentsHeight != model.segmentsHeight
                    || newConeModel.segmentsRadial != model.segmentsRadial
                    || newConeModel.openEnded != model.openEnded
                    || newConeModel.arc != model.arc;
        }
    }
}
