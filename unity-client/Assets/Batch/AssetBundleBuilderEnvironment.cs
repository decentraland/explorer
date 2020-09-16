namespace DCL
{
    public class AssetBundleBuilderEnvironment
    {
        public virtual IDirectory directory { get; private set; }
        public virtual IFile file { get; private set; }
        public virtual IAssetDatabase assetDatabase { get; private set; }
        public virtual IWebRequest webRequest { get; private set; }

        public static AssetBundleBuilderEnvironment CreateWithDefaultImplementations()
        {
            return new AssetBundleBuilderEnvironment();
        }
    }
}