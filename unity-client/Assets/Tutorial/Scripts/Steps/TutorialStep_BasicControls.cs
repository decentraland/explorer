using DCL.Interface;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to the very basic controls.
    /// </summary>
    public class TutorialStep_BasicControls : TutorialStep
    {
        [SerializeField] Button okButton;
        [SerializeField] Button goToGenesisButton;

        private bool stepIsFinished = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            okButton.onClick.AddListener(OnOkButtonClick);
            goToGenesisButton.onClick.AddListener(OnGoToGenesisButtonClick);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
        }

        private void OnOkButtonClick()
        {
            stepIsFinished = true;
        }

        private void OnGoToGenesisButtonClick()
        {
            CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;

            WebInterface.GoTo(0, 0);

            if (tutorialController != null)
                tutorialController.SetTutorialDisabled();
        }

        private void RendererState_OnChange(bool current, bool previous)
        {
            if (current)
            {
                CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
                if (SceneController.i != null)
                    SceneController.i.OnSortScenes += SceneController_OnSortScenes;
            }
        }

        private void SceneController_OnSortScenes()
        {
            SceneController.i.OnSortScenes -= SceneController_OnSortScenes;

            if (tutorialController != null)
                tutorialController.SetTutorialEnabled(false.ToString());
        }
    }
}