namespace DCL
{
    public class EnvironmentV2
    {
        public static Model i = new Model();

        public static void Setup(System.Func<MessagingContext> messagingBuilder,
            System.Func<PlatformContext> platformBuilder,
            System.Func<WorldRuntimeContext> worldRuntimeBuilder)
        {
            i.messagingBuilder = messagingBuilder;
            i.platformBuilder = platformBuilder;
            i.worldRuntimeBuilder = worldRuntimeBuilder;

            i = new Model(messagingBuilder, platformBuilder, worldRuntimeBuilder);
        }

        public static void Reset()
        {
            i.Dispose();

            Setup(i.messagingBuilder,
                i.platformBuilder,
                i.worldRuntimeBuilder);
        }

        public class Model
        {
            internal System.Func<MessagingContext> messagingBuilder;
            internal System.Func<PlatformContext> platformBuilder;
            internal System.Func<WorldRuntimeContext> worldRuntimeBuilder;

            public readonly MessagingContext messaging;
            public readonly PlatformContext platform;
            public readonly WorldRuntimeContext world;

            public Model(System.Func<MessagingContext> messaging = null,
                System.Func<PlatformContext> platform = null,
                System.Func<WorldRuntimeContext> world = null)
            {
                if (messaging != null)
                    this.messaging = messaging();

                if (platform != null)
                    this.platform = platform();

                if (world != null)
                    this.world = world();
            }

            public void Dispose()
            {
                messaging.Dispose();
                world.Dispose();
                platform.Dispose();
            }
        }
    }
}