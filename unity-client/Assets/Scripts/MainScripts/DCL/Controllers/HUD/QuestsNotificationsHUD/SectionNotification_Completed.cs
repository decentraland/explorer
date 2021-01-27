using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestsNotifications
{
    public class SectionNotification_Completed : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI sectionName;

        public void Populate(QuestSection section)
        {
            sectionName.text = section.name;
        }
    }
}