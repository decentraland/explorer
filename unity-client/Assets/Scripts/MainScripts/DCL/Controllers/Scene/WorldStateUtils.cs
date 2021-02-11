﻿using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public static class WorldStateUtils
    {
        public static bool IsGlobalScene(string sceneId)
        {
            var worldState = Environment.i.world.state;

            if (worldState.TryGetScene(sceneId, out IParcelScene scene))
            {
                return scene is GlobalScene;
            }

            return false;
        }

        public static string TryToGetSceneCoordsID(string id)
        {
            var worldState = Environment.i.world.state;

            if (worldState.loadedScenes.ContainsKey(id))
                return worldState.loadedScenes[id].sceneData.basePosition.ToString();

            return id;
        }

        public static Vector3 ConvertUnityToScenePosition(Vector3 pos, IParcelScene scene = null)
        {
            if (scene == null)
            {
                var worldState = Environment.i.world.state;

                string sceneId = worldState.currentSceneId;

                if (!string.IsNullOrEmpty(sceneId) && worldState.loadedScenes.ContainsKey(sceneId))
                    scene = worldState.loadedScenes[worldState.currentSceneId];
                else
                    return pos;
            }

            Vector3 worldPosition = PositionUtils.UnityToWorldPosition(pos);
            return worldPosition - Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
        }

        public static Vector3 ConvertSceneToUnityPosition(Vector3 pos, IParcelScene scene = null)
        {
            return ConvertPointInSceneToUnityPosition(pos, scene);
        }

        public static Vector3 ConvertScenePositionToUnityPosition(IParcelScene scene = null)
        {
            return ConvertPointInSceneToUnityPosition(Vector3.zero, scene);
        }

        public static Vector3 ConvertPointInSceneToUnityPosition(Vector3 pos, IParcelScene scene = null)
        {
            if (scene == null)
            {
                IWorldState worldState = Environment.i.world.state;
                string sceneId = worldState.currentSceneId;

                if (!string.IsNullOrEmpty(sceneId) && worldState.loadedScenes.ContainsKey(sceneId))
                    scene = worldState.loadedScenes[worldState.currentSceneId];
                else
                    return pos;
            }

            return ConvertPointInSceneToUnityPosition(pos, new Vector2Int(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y));
        }

        public static Vector3 ConvertPointInSceneToUnityPosition(Vector3 pos, Vector2Int scenePoint)
        {
            Vector3 scenePosition = Utils.GridToWorldPosition(scenePoint.x, scenePoint.y) + pos;
            Vector3 worldPosition = PositionUtils.WorldToUnityPosition(scenePosition);

            return worldPosition;
        }

        public static bool IsCharacterInsideScene(IParcelScene scene)
        {
            return scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition);
        }
    }
}