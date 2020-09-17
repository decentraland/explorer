namespace DCL
{
    public interface IWebRequest
    {
        byte[] Get(string url);
        void GetAsync(string url, System.Action<byte[]> OnCompleted);
    }
}