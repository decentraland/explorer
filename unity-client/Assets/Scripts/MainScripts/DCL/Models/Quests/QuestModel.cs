using System.Linq;
using DCL.Helpers;

public static class QuestLiterals
{
    public static class Status
    {
        public static string BLOCKED = "blocked";
        public static string NOT_STARTED = "not_started";
        public static string ON_GOING = "on_going";
        public static string COMPLETED = "completed";
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
    public string icon;
    public QuestSection[] sections;

    public bool TryGetSection(string sectionId, out QuestSection section)
    {
        section = sections.FirstOrDefault(x => x.id == sectionId);
        return section != null;
    }

    public bool canBePinned => !isCompleted && status != QuestLiterals.Status.BLOCKED;
    public bool isCompleted => status == QuestLiterals.Status.COMPLETED;
    public float progress => sections.Average(x => x.progress);

    public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<QuestModel>(json); }

    protected bool Equals(QuestModel other)
    {
        return id == other.id && name == other.name && description == other.description && thumbnail_entry == other.thumbnail_entry && status == other.status && thumbnail_banner == other.thumbnail_banner && icon == other.icon && Equals(sections, other.sections);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((QuestModel) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = (id != null ? id.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (name != null ? name.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (description != null ? description.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (thumbnail_entry != null ? thumbnail_entry.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (status != null ? status.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (thumbnail_banner != null ? thumbnail_banner.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (icon != null ? icon.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (sections != null ? sections.GetHashCode() : 0);
            return hashCode;
        }
    }
}
