﻿using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers
{
    public interface IWorldBlockersController
    {
        void Initialize(ISceneHandler sceneHandler, IBlockerInstanceHandler blockerInstanceHandler, DCLCharacterPosition characterPosition);
        void InitializeWithDefaultDependencies(ISceneHandler sceneHandler, DCLCharacterPosition characterPosition);
        void SetupWorldBlockers();
        void SetEnabled(bool targetValue);
        void Dispose();
    }

    /// <summary>
    /// This class is the domain-specific glue for BlockerInstanceHandler.
    /// <br/><br/>
    /// Responsibilities:<br/>
    /// - Spawning blockers depending on scene state<br/>
    /// - Moving blockers when the world is repositioned<br/>
    /// - Handling lifecycle of BlockerInstanceHandler<br/>
    /// </summary>
    public class WorldBlockersController : IWorldBlockersController
    {
        public bool enabled = true;

        Transform blockersParent;

        ISceneHandler sceneHandler;
        IBlockerInstanceHandler blockerInstanceHandler;
        DCLCharacterPosition characterPosition;

        HashSet<Vector2Int> blockersToRemove = new HashSet<Vector2Int>();
        HashSet<Vector2Int> blockersToAdd = new HashSet<Vector2Int>();

        static Vector2Int[] aroundOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1)
        };

        public void Initialize(ISceneHandler sceneHandler, IBlockerInstanceHandler blockerInstanceHandler, DCLCharacterPosition characterPosition)
        {
            this.blockerInstanceHandler = blockerInstanceHandler;
            this.sceneHandler = sceneHandler;
            this.characterPosition = characterPosition;

            blockerInstanceHandler.SetParent(blockersParent);
            characterPosition.OnPrecisionAdjust += OnWorldReposition;
        }

        public WorldBlockersController()
        {
            blockersParent = new GameObject("WorldBlockers").transform;
            blockersParent.position = Vector3.zero;
        }

        public static WorldBlockersController CreateWithDefaultDependencies(ISceneHandler sceneHandler, DCLCharacterPosition characterPosition)
        {
            var worldBlockersController = new WorldBlockersController();
            worldBlockersController.InitializeWithDefaultDependencies(sceneHandler, characterPosition);
            return worldBlockersController;
        }

        public void InitializeWithDefaultDependencies(ISceneHandler sceneHandler, DCLCharacterPosition characterPosition)
        {
            var blockerAnimationHandler = new BlockerAnimationHandler();
            var blockerInstanceHandler = new BlockerInstanceHandler();

            blockerInstanceHandler.Initialize(
                characterPosition,
                blockerAnimationHandler
            );

            Initialize(
                sceneHandler,
                blockerInstanceHandler,
                characterPosition);
        }

        public void SetupWorldBlockers()
        {
            if (!enabled) return;

            SetupWorldBlockers(sceneHandler.GetAllLoadedScenesCoords());
        }

        public void SetEnabled(bool targetValue)
        {
            enabled = targetValue;

            if (!enabled)
                blockerInstanceHandler.DestroyAllBlockers();
        }

        void OnWorldReposition(DCLCharacterPosition charPos)
        {
            // Blockers parent original position
            blockersParent.position = charPos.WorldToUnityPosition(Vector3.zero);
        }

        public void Dispose()
        {
            characterPosition.OnPrecisionAdjust -= OnWorldReposition;
            blockerInstanceHandler.DestroyAllBlockers();

            if (blockersParent != null)
                Object.Destroy(blockersParent.gameObject);
        }

        internal void SetupWorldBlockers(HashSet<Vector2Int> allLoadedParcelCoords)
        {
            if (allLoadedParcelCoords.Count == 0) return;

            blockersToRemove.Clear();
            blockersToAdd.Clear();

            var blockers = blockerInstanceHandler.GetBlockers();

            // Detect blockers to be removed
            foreach (var item in blockers)
            {
                if (allLoadedParcelCoords.Contains(item.Key))
                {
                    blockersToRemove.Add(item.Key);
                }
                else
                {
                    bool foundAroundLoadedScenes = false;
                    for (int i = 0; i < aroundOffsets.Length; i++)
                    {
                        Vector2Int offset = aroundOffsets[i];
                        Vector2Int checkedPosition = new Vector2Int(item.Key.x + offset.x, item.Key.y + offset.y);

                        if (allLoadedParcelCoords.Contains(checkedPosition))
                        {
                            foundAroundLoadedScenes = true;
                            break;
                        }
                    }

                    if (!foundAroundLoadedScenes)
                        blockersToRemove.Add(item.Key);
                }
            }

            // Detect missing blockers to be added
            using (var it = allLoadedParcelCoords.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    Vector2Int pos = it.Current;

                    for (int i = 0; i < aroundOffsets.Length; i++)
                    {
                        Vector2Int offset = aroundOffsets[i];
                        Vector2Int checkedPosition = new Vector2Int(pos.x + offset.x, pos.y + offset.y);

                        if (!allLoadedParcelCoords.Contains(checkedPosition) && !blockers.ContainsKey(checkedPosition))
                        {
                            blockersToAdd.Add(checkedPosition);
                        }
                    }
                }
            }

            // Remove extra blockers
            foreach (var coords in blockersToRemove)
            {
                blockerInstanceHandler.HideBlocker(coords, false);
            }

            // Add missing blockers
            foreach (var coords in blockersToAdd)
            {
                blockerInstanceHandler.ShowBlocker(coords);
            }
        }
    }
}