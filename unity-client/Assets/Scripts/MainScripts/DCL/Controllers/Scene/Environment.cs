﻿using DCL.Configuration;
using DCL.Controllers;
using DCL.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public class Environment
    {
        public static readonly Environment i = new Environment();

        public readonly MessagingControllersManager messagingControllersManager;
        public readonly PointerEventsController pointerEventsController;
        public readonly MemoryManager memoryManager;
        public WorldBlockersController worldBlockersController { get; private set; }
        public ICullingController cullingController { get; private set; }
        public InteractionHoverCanvasController interactionHoverCanvasController { get; private set; }
        public Clipboard clipboard { get; }

        private bool initialized;

        private Environment()
        {
            messagingControllersManager = new MessagingControllersManager();
            pointerEventsController = new PointerEventsController();
            memoryManager = new MemoryManager();
            clipboard = Clipboard.Create();
        }

        public void Initialize(IMessageProcessHandler messageHandler, ISceneHandler sceneHandler)
        {
            if (initialized)
                return;

            messagingControllersManager.Initialize(messageHandler);
            pointerEventsController.Initialize();
            memoryManager.Initialize();
            cullingController = CullingController.Create();
            worldBlockersController = WorldBlockersController.CreateWithDefaultDependencies(sceneHandler, DCLCharacterController.i.characterPosition);

            initialized = true;
        }

        public void SetInteractionHoverCanvasController(InteractionHoverCanvasController controller)
        {
            interactionHoverCanvasController = controller;
        }

        public void Cleanup()
        {
            if (!initialized)
                return;

            initialized = false;

            messagingControllersManager.Cleanup();
            memoryManager.CleanupPoolsIfNeeded(true);
            pointerEventsController.Cleanup();
            worldBlockersController.Dispose();
        }

        public void Restart(IMessageProcessHandler messageHandler, ISceneHandler sceneHandler)
        {
            Cleanup();
            Initialize(messageHandler, sceneHandler);
        }
    }
}