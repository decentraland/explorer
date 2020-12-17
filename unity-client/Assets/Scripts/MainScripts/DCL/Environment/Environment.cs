using System.ComponentModel;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DCL
{
    public class Environment
    {
        public static Model i = new Model();
        private static System.Func<MessagingContext> messagingBuilder;
        private static System.Func<PlatformContext> platformBuilder;
        private static System.Func<WorldRuntimeContext> worldRuntimeBuilder;

        public static void SetupWithDefaults()
        {
            Setup(
                messagingBuilder: MessagingContextFactory.CreateDefault,
                platformBuilder: PlatformContextFactory.CreateDefault,
                worldRuntimeBuilder: WorldRuntimeContextFactory.CreateDefault
            );
        }

        public static void Setup(System.Func<MessagingContext> messagingBuilder,
            System.Func<PlatformContext> platformBuilder,
            System.Func<WorldRuntimeContext> worldRuntimeBuilder)
        {
            Environment.messagingBuilder = messagingBuilder;
            Environment.platformBuilder = platformBuilder;
            Environment.worldRuntimeBuilder = worldRuntimeBuilder;
            Setup();
        }

        public static void Setup()
        {
            i = new Model(messagingBuilder, platformBuilder, worldRuntimeBuilder);
            Initialize();
        }

        public static void Initialize()
        {
            Model model = i;

            //TODO(Brian): We can move to a RAII scheme + promises later to make this
            //             more scalable.

            // World context systems
            model.world.sceneController.Initialize();
            model.world.pointerEventsController.Initialize();
            model.world.state.Initialize();
            model.world.blockersController.InitializeWithDefaultDependencies(
                model.world.state,
                DCLCharacterController.i.characterPosition);
            model.world.sceneBoundsChecker.Start();

            // Platform systems
            model.platform.memoryManager.Initialize();
            model.platform.parcelScenesCleaner.Start();
            model.platform.cullingController.Start();

            // Messaging systems
            model.messaging.manager.Initialize(i.world.sceneController);
        }

        public static void Reset()
        {
            Dispose();
            Setup();
        }

        public static void Dispose()
        {
            i.Dispose();
        }

        public class Model
        {
            public readonly MessagingContext messaging;
            public readonly PlatformContext platform;
            public readonly WorldRuntimeContext world;

            public Model(System.Func<MessagingContext> messagingBuilder = null,
                System.Func<PlatformContext> platformBuilder = null,
                System.Func<WorldRuntimeContext> worldBuilder = null)
            {
                messagingBuilder = messagingBuilder ?? MessagingContextFactory.CreateDefault;
                platformBuilder = platformBuilder ?? PlatformContextFactory.CreateDefault;
                worldBuilder = worldBuilder ?? WorldRuntimeContextFactory.CreateDefault;

                this.messaging = messagingBuilder();
                this.platform = platformBuilder();
                this.world = worldBuilder();
            }

            public void Dispose()
            {
                messaging?.Dispose();
                world?.Dispose();
                platform?.Dispose();
            }
        }
    }
}