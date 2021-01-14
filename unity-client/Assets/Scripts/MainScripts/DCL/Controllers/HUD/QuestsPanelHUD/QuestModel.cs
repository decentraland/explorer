namespace DCL.Huds
{
    [System.Serializable]
    public class QuestModel
    {
        public string id;
        public string name;
        public string description;
        public string thumbnail;
        public string jumpAction;
        public QuestTask[] tasks;
    }

    [System.Serializable]
    public class QuestTask
    {
        public string id;
        public string name;
        public QuestStep[] steps;
    }

    [System.Serializable]
    public class QuestStep
    {
        public string id;
        public string type;
        public string payload;
    }
}
