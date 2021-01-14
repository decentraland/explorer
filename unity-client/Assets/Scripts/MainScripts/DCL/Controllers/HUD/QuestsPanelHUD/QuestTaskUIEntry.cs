using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestPanel
{
    public class QuestTaskUIEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI taskName;
        [SerializeField] private RectTransform stepsContainer;
        [SerializeField] private QuestStepUIFactory factory;

        public void Populate(QuestPanelTask task)
        {
            CleanUpStepsList(); //TODO: Reuse already instantiated steps
            taskName.text = task.name;
            foreach (QuestPanelStep step in task.steps)
            {
                CreateStep(step);
            }
        }

        internal void CreateStep(QuestPanelStep step)
        {
            GameObject prefab = factory.GetPrefab(step.type);
            if (prefab == null)
            {
                Debug.LogError($"Type: {step.type} was not found in QuestStepFactory");
                return;
            }

            var stepUIEntry = Instantiate(prefab, stepsContainer).GetComponent<IQuestStepUIEntry>();
            stepUIEntry.Populate(step.payload);
        }

        internal void CleanUpStepsList()
        {
            while (stepsContainer.childCount > 0)
            {
                Destroy(stepsContainer.GetChild(0).gameObject);
            }
        }
    }
}