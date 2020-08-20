namespace DCL
{
    public class Singleton<T> where T : class, new()
    {
        private static readonly T i = new T();

    }
}
