using Variables.RealmsInfo;

namespace DCL
{
    public static class DataStore
    {
        static public readonly CurrentRealmVariable playerRealm = new CurrentRealmVariable();
        static public readonly RealmsVariable realmsInfo = new RealmsVariable();
        static public readonly DebugConfig debugConfig = new DebugConfig();
        static public readonly BaseVariable<bool> isSignUpFlow = new BaseVariable<bool>();

        public static class Catalog
        {
            static public readonly BaseDictionary<string, WearableItem> wearables = new BaseDictionary<string, WearableItem>();
            static public readonly BaseDictionary<string, Item> items = new BaseDictionary<string, Item>();
        }
    }
}