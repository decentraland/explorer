using DCL.Tutorial;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Tutorial_Tests
{
    public class TutorialControllerShould : TestsBase
    {
        private int currentStepIndex = 0;
        private List<TutorialStep> currentSteps = new List<TutorialStep>();

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            AddFakeTutorialSteps();
        }

        protected override IEnumerator TearDown()
        {
            tutorialController.SetTutorialDisabled();
            tutorialController.stepsOnGenesisPlaza.Clear();
            tutorialController.stepsFromDeepLink.Clear();
            tutorialController.stepsOnGenesisPlazaAfterDeepLink.Clear();
            currentSteps.Clear();
            currentStepIndex = 0;

            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromGenesisPlazaCorrectly()
        {
            ConfigureTutorialForGenesisPlaza();

            yield return CoroutineStarter.Start(tutorialController.StartTutorialFromStep(0));

            Assert.IsTrue(tutorialController.markTutorialAsCompleted);
            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromDeepLinkCorrectly()
        {
            ConfigureTutorialForDeepLink();

            yield return CoroutineStarter.Start(tutorialController.StartTutorialFromStep(0));

            Assert.IsFalse(tutorialController.markTutorialAsCompleted);
            Assert.IsTrue(tutorialController.alreadyOpenedFromDeepLink);
            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromGenesisPlazaAfterDeepLinkCorrectly()
        {
            ConfigureTutorialForGenesisPlazaAfterDeepLink();

            yield return CoroutineStarter.Start(tutorialController.StartTutorialFromStep(0));

            Assert.IsTrue(tutorialController.markTutorialAsCompleted);
            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
        }

        [Test]
        public void ShowHideTutorialTeacherCorrectly()
        {
            tutorialController.ShowTeacher3DModel(true);
            Assert.IsTrue(tutorialController.teacherRawImage.gameObject.activeSelf);

            tutorialController.ShowTeacher3DModel(false);
            Assert.IsFalse(tutorialController.teacherRawImage.gameObject.activeSelf);
        }

        [Test]
        public void SetTutorialTeacherPositionCorrectly()
        {
            Vector3 newTeacherPosition = new Vector3(10, 30, 0);
            tutorialController.SetTeacherPosition(newTeacherPosition, false);
            Assert.IsTrue(tutorialController.teacherRawImage.rectTransform.position == newTeacherPosition);
        }

        private void AddFakeTutorialSteps()
        {
            currentStepIndex = 0;

            for (int i = 0; i < 5; i++)
            {
                tutorialController.stepsOnGenesisPlaza.Add(new TutorialStep_Mock
                {
                    customOnStepStart = WaitForOnStepStart,
                    customOnStepExecute = WaitForOnStepExecute,
                    customOnStepPlayAnimationForHidding = WaitForOnStepPlayAnimationForHidding,
                    customOnStepFinished = WaitForOnStepFinished
                });
            }

            for (int i = 0; i < 5; i++)
            {
                tutorialController.stepsFromDeepLink.Add(new TutorialStep_Mock
                {
                    customOnStepStart = WaitForOnStepStart,
                    customOnStepExecute = WaitForOnStepExecute,
                    customOnStepPlayAnimationForHidding = WaitForOnStepPlayAnimationForHidding,
                    customOnStepFinished = WaitForOnStepFinished
                });
            }

            for (int i = 0; i < 5; i++)
            {
                tutorialController.stepsOnGenesisPlazaAfterDeepLink.Add(new TutorialStep_Mock
                {
                    customOnStepStart = WaitForOnStepStart,
                    customOnStepExecute = WaitForOnStepExecute,
                    customOnStepPlayAnimationForHidding = WaitForOnStepPlayAnimationForHidding,
                    customOnStepFinished = WaitForOnStepFinished
                });
            }
        }

        private void ConfigureTutorialForGenesisPlaza()
        {
            tutorialController.playerIsInGenesisPlaza = true;
            tutorialController.alreadyOpenedFromDeepLink = false;
            tutorialController.isRunning = true;
            tutorialController.markTutorialAsCompleted = false;

            currentSteps = tutorialController.stepsOnGenesisPlaza;
        }

        private void ConfigureTutorialForDeepLink()
        {
            tutorialController.playerIsInGenesisPlaza = false;
            tutorialController.openedFromDeepLink = true;
            tutorialController.isRunning = true;
            tutorialController.markTutorialAsCompleted = true;

            currentSteps = tutorialController.stepsFromDeepLink;
        }

        private void ConfigureTutorialForGenesisPlazaAfterDeepLink()
        {
            tutorialController.playerIsInGenesisPlaza = true;
            tutorialController.alreadyOpenedFromDeepLink = true;
            tutorialController.isRunning = true;
            tutorialController.markTutorialAsCompleted = false;

            currentSteps = tutorialController.stepsOnGenesisPlazaAfterDeepLink;
        }

        private void WaitForOnStepStart()
        {
            CheckRunningStep();
        }

        private IEnumerator WaitForOnStepExecute()
        {
            CheckRunningStep();
            yield return null;
        }

        private IEnumerator WaitForOnStepPlayAnimationForHidding()
        {
            CheckRunningStep();
            yield return null;
        }

        private void WaitForOnStepFinished()
        {
            CheckRunningStep();
            currentStepIndex++;
        }

        private void CheckRunningStep()
        {
            Assert.IsTrue(tutorialController.isRunning);
            Assert.IsNotNull(tutorialController.runningStep);
            Assert.IsTrue(currentSteps[currentStepIndex] == tutorialController.runningStep);
        }
    }
}