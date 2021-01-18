namespace DCL.Huds.QuestPanel
{
    [System.Serializable]
    public class QuestModel
    {
        public string id;
        public string name;
        public string description;
        public string thumbnail_entry;
        public string thumbnail_banner;
        public string jumpAction;
        public QuestSection[] sections;
    }

}
