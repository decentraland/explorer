namespace DCL
{
    public class MessagingContext : System.IDisposable
    {
        public readonly MessagingControllersManager manager;

        public MessagingContext(MessagingControllersManager manager)
        {
            this.manager = manager;
        }

        public void Dispose()
        {
            manager.Dispose();
        }
    }
}