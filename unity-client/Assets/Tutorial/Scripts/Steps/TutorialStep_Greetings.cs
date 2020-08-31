using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    public class TutorialStep_Greetings : TutorialStep
    {
        [SerializeField] Button okButton;
        [SerializeField] TMP_Text titleText;

        private bool stepIsFinished = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            titleText.text = titleText.text.Replace("{userName}", UserProfile.GetOwnUserProfile().userName);

            okButton.onClick.AddListener(onOkButtonClick);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
        }

        private void onOkButtonClick()
        {
            stepIsFinished = true;
        }
    }
}