using Variables.RealmsInfo;

namespace DCL
{
    public static class DataStore
    {
        static public readonly CurrentRealmVariable playerRealm = new CurrentRealmVariable();
        static public readonly RealmsVariable realmsInfo = new RealmsVariable();
        static public readonly DebugConfig debugConfig = new DebugConfig();
        static public readonly BaseVariable<bool> isSignUpFlow = new BaseVariable<bool>();

        public static class BuilderInWorld
        {
            static public readonly BaseDictionary<string, CatalogItem> catalogItemDict = new BaseDictionary<string, CatalogItem>();
            static public readonly BaseDictionary<string, CatalogItemPack> catalogItemPackDict = new BaseDictionary<string, CatalogItemPack>();
            static public readonly BaseDictionary<string, CatalogItemPack> catalogItemCategoryDict = new BaseDictionary<string, CatalogItemPack>();
        }
    }
}