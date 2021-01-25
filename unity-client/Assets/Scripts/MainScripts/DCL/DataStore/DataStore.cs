using Variables.RealmsInfo;

namespace DCL
{
    public static class DataStore
    {
        static public readonly CurrentRealmVariable playerRealm = new CurrentRealmVariable();
        static public readonly RealmsVariable realmsInfo = new RealmsVariable();
        static public readonly DebugConfig debugConfig = new DebugConfig();
        static public readonly BaseVariable<bool> isSignUpFlow = new BaseVariable<bool>();

        public static class Quests
        {
            static public readonly BaseDictionary<string, QuestModel> quests = new BaseDictionary<string, QuestModel>();
            static public readonly BaseCollection<string> pinnedQuests = new BaseCollection<string>();
        }
    }
}