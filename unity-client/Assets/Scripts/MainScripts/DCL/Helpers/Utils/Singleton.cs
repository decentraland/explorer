namespace DCL
{
    public class Singleton<T> where T : class, new()
    {
        private static T instance = null;

        public static T i
        {
            get
            {
                if (instance == null)
                    instance = new T();

                return instance;
            }
        }

        public virtual void Initialize()
        {
            //NOTE(Brian): You have to call this doing Singleton.i.Initialize().
            //             This by itself triggers the getter and creates the instance.
            //             Sorry for the "clever" code.
        }
    }
}
