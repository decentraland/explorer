using System;
using System.Linq;
using DCL.Helpers;

public static class QuestsLiterals
{
    public static class Status
    {
        public static string BLOCKED = "blocked";
        public static string NOT_STARTED = "not_started";
        public static string ON_GOING = "on_going";
        public static string COMPLETED = "completed";
        public static string FAILED = "failed";
    }

    public static class RewardStatus
    {
        public static string NOT_GIVEN = "not_given";
        public static string OK = "ok";
        public static string ALREADY_GIVEN = "already_given";
        public static string TASK_ALREADY_COMPLETED = "task_already_completed";
        public static string FAILED = "failed";
    }
}

[System.Serializable]
public class QuestModel : BaseModel
{
    public string id;
    public string name;
    public string description;
    public string thumbnail_entry;
    public string status;
    public string thumbnail_banner;
    public QuestSection[] sections;
    public DateTime assignmentTime = DateTime.Now; //TODO remove this once kernel send the data properly
    public DateTime completionTime = DateTime.Now; //TODO remove this once kernel send the data properly
    public QuestReward[] rewards;

    [NonSerialized]
    public float oldProgress = 0;

    public bool TryGetSection(string sectionId, out QuestSection section)
    {
        section = sections.FirstOrDefault(x => x.id == sectionId);
        return section != null;
    }

    public bool TryGetReward(string rewardId, out QuestReward reward)
    {
        reward = rewards.FirstOrDefault(x => x.id == rewardId);
        return reward != null;
    }

    public bool canBePinned => !isCompleted && status != QuestsLiterals.Status.BLOCKED;
    public bool isCompleted => status == QuestsLiterals.Status.COMPLETED;
    public bool hasAvailableTasks => sections.Any(x => x.tasks.Any(y => y.status != QuestsLiterals.Status.BLOCKED));
    public bool justProgressed => sections.Any(x => x.tasks.Any(y => y.status != QuestsLiterals.Status.BLOCKED && y.justProgressed));
    public float progress => sections.Average(x => x.progress);

    public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<QuestModel>(json); }
}