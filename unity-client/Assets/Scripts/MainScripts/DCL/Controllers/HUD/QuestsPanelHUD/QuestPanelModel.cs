namespace DCL.Huds.QuestPanel
{
    [System.Serializable]
    public class QuestPanelModel
    {
        public string id;
        public string name;
        public string description;
        public string thumbnail;
        public string jumpAction;
        public QuestPanelSection[] sections;
    }

    [System.Serializable]
    public class QuestPanelSection
    {
        public string id;
        public string name;
        public QuestPanelTask[] tasks;
    }

    [System.Serializable]
    public class QuestPanelTask
    {
        public string id;
        public string type;
        public string payload;
    }
}
