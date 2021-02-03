using System.Linq;

[System.Serializable]
public class QuestModel
{
    public string id;
    public string name;
    public string description;
    public string thumbnail_entry;
    public string thumbnail_banner;
    public string icon;
    public QuestSection[] sections;

    public bool TryGetSection(string sectionId, out QuestSection section)
    {
        section = sections.FirstOrDefault(x => x.id == sectionId);
        return section != null;
    }

    public bool isCompleted => sections.All(x => x.progress >= 1);
    public float progress => sections.Average(x => x.progress);
}
