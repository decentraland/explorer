namespace DCL
{
    class Environment : Singleton<Environment>
    {
        public MessagingControllersManager messagingControllersManager { get; }

        /*
         * TODO: Continue moving static beans to this class
         * 
        MemoryManager memoryManager;
        PointerEventsController pointerEventsController;
        ParcelScenesCleaner parcelScenesCleaner; // This is a static member of ParcelScene
        PoolManager poolManager; // This should be created through a Factory, and that factopry should execute the code in the method EnsureEntityPool

        */

        public Environment()
        {
            messagingControllersManager = new MessagingControllersManager();
        }

        public void Initialize(IMessageProcessHandler messageHandler)
        {
            messagingControllersManager.Initialize(messageHandler);
        }

        public void Restart(IMessageProcessHandler messageHandler)
        {
            messagingControllersManager.Cleanup();

            this.Initialize(messageHandler);
        }
    }
}