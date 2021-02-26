using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using UnityEngine;

namespace DCL.Components
{
    public class CylinderShape : ParametrizedShape<CylinderShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
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

            public float radiusTop = 1f;
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
                return radiusTop.Equals(other.radiusTop) && radiusBottom.Equals(other.radiusBottom) &&
                       segmentsHeight.Equals(other.segmentsHeight) && segmentsRadial.Equals(other.segmentsRadial) &&
                       openEnded == other.openEnded && Nullable.Equals(radius, other.radius) && arc.Equals(other.arc);
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
        }

        public CylinderShape(ParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.CYLINDER_SHAPE;
        }

        public override Mesh GenerateGeometry()
        {
            var model = (Model)this.model;
            return PrimitiveMeshBuilder.BuildCylinder(50, model.radiusTop, model.radiusBottom, 2f, 0f, true, false);
        }

        protected override bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            if(currentMesh == null) return true;

            Model newCylinderModel = newModel as Model;
            var model = (Model)this.model;
            return  newCylinderModel.radius != model.radius
                    || newCylinderModel.radiusTop != model.radiusTop
                    || newCylinderModel.radiusBottom != model.radiusBottom
                    || newCylinderModel.segmentsHeight != model.segmentsHeight
                    || newCylinderModel.segmentsRadial != model.segmentsRadial
                    || newCylinderModel.openEnded != model.openEnded
                    || newCylinderModel.arc != model.arc;
        }
    }
}