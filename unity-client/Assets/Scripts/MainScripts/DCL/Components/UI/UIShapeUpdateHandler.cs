﻿using System.Collections;

namespace DCL.Components
{
    public class UIShapeUpdateHandler<ReferencesContainerType, ModelType> : ComponentUpdateHandler
        where ReferencesContainerType : UIReferencesContainer
        where ModelType : UIShape.Model

    {
        public UIShape<ReferencesContainerType, ModelType> uiShapeOwner;
        public UIShapeUpdateHandler(IComponent owner) : base(owner)
        {
            uiShapeOwner = owner as UIShape<ReferencesContainerType, ModelType>;
        }

        public override IEnumerator ApplyChangesWrapper(string newJson)
        {
            uiShapeOwner.PreApplyChanges(newJson);

            var enumerator = base.ApplyChangesWrapper(newJson);

            if (enumerator != null)
            {
                yield return enumerator;
            }
        }
    }
}
