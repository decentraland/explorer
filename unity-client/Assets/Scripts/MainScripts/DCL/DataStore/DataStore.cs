using Variables.RealmsInfo;

namespace DCL
{
    public class DataStore
    {
        private static DataStore instance = new DataStore();
        public static DataStore i { get => instance; }
        public static void Clear() => instance = new DataStore();

        public readonly CurrentRealmVariable playerRealm = new CurrentRealmVariable();
        public readonly RealmsVariable realmsInfo = new RealmsVariable();
        public readonly DebugConfig debugConfig = new DebugConfig();
        public readonly BaseVariable<bool> isSignUpFlow = new BaseVariable<bool>();
        public readonly BaseDictionary<string, WearableItem> wearables = new BaseDictionary<string, WearableItem>();
        public readonly BaseDictionary<string, Item> items = new BaseDictionary<string, Item>();

        public static class BuilderInWorld
        {
            static public readonly BaseDictionary<string, CatalogItem> catalogItemDict = new BaseDictionary<string, CatalogItem>();
            static public readonly BaseDictionary<string, CatalogItemPack> catalogItemPackDict = new BaseDictionary<string, CatalogItemPack>();
            static public readonly BaseDictionary<string, CatalogItemPack> catalogItemCategoryDict = new BaseDictionary<string, CatalogItemPack>();
        }
    }
}