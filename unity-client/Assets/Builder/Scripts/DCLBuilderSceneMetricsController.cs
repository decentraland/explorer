﻿using DCL;
using DCL.Controllers;
using DCL.Models;

namespace Builder
{
    public class DCLBuilderSceneMetricsController : SceneMetricsController
    {
        public DCLBuilderSceneMetricsController(ParcelScene sceneOwner) : base(sceneOwner)
        {
            Enable();
        }

        protected override void OnEntityAdded(DecentralandEntity e)
        {
            e.OnMeshesInfoUpdated += OnEntityMeshInfoUpdated;
            e.OnMeshesInfoCleaned += OnEntityMeshInfoCleaned;
        }

        protected override void OnEntityRemoved(DecentralandEntity e)
        {
            e.OnMeshesInfoUpdated -= OnEntityMeshInfoUpdated;
            e.OnMeshesInfoCleaned -= OnEntityMeshInfoCleaned;

            if (!e.components.ContainsKey(CLASS_ID_COMPONENT.SMART_ITEM))
            {
                SubstractMetrics(e);
                model.entities = entitiesMetrics.Count;
                isDirty = true;
            }
        }

        protected override void OnEntityMeshInfoUpdated(DecentralandEntity entity)
        {
            //builder should only check scene limits for not smart items entities
            if (!entity.components.ContainsKey(CLASS_ID_COMPONENT.SMART_ITEM))
            {
                AddOrReplaceMetrics(entity);
                model.entities = entitiesMetrics.Count;
                isDirty = true;
            }
        }
    }
}