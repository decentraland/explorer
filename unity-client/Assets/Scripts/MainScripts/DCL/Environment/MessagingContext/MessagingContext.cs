namespace DCL
{
    public class MessagingContext : System.IDisposable
    {
        public readonly MessagingControllersManager messagingControllersManager;

        public MessagingContext(MessagingControllersManager messagingControllersManager)
        {
            this.messagingControllersManager = messagingControllersManager;
        }

        public void Dispose()
        {
            messagingControllersManager.Cleanup();
            //messageQueueHandler.Dispose();
        }
    }
}