using System.Linq;
using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestsNotifications
{
    public class SectionNotification_Unlocked : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI sectionName;
        [SerializeField] private TextMeshProUGUI taskName;

        public void Populate(QuestSection section)
        {
            sectionName.text = section.name;
            taskName.text = section.tasks.FirstOrDefault()?.name;
        }
    }
}