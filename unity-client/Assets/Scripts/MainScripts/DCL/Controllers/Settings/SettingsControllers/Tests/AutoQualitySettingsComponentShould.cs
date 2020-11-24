using System.Collections;
using DCL;
using DCL.SettingsData;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using QualitySettings = DCL.SettingsData.QualitySettings;

namespace Tests
{
    public class AutoQualitySettingsComponentShould : TestsBase
    {
        private AutoQualitySettingsComponent component;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            component = CreateTestGameObject("controllerHolder").AddComponent<AutoQualitySettingsComponent>();
            Settings.i.autoqualitySettings = ScriptableObject.CreateInstance<QualitySettingsData>();
            component.qualitySettings.Set(new []
            {
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
            });
        }

        [Test]
        public void LowerTheQualityOnPerformanceDrop()
        {
            component.currentQualityIndex = 2;
            //component.evaluator = Substitute.For<IAutoQualitySettingsEvaluator>();
            //component.evaluator.Evaluate(null).ReturnsForAnyArgs( -1);

            //component.EvaluateQuality();

            Assert.AreEqual(1, component.currentQualityIndex);
        }

        [Test]
        public void MaintainTheQualityOnAcceptablePerformance()
        {
            component.currentQualityIndex = 2;
            //component.evaluator = Substitute.For<IAutoQualitySettingsEvaluator>();
            //component.evaluator.Evaluate(null).ReturnsForAnyArgs( 0);

            //component.EvaluateQuality();

            Assert.AreEqual(2, component.currentQualityIndex);
        }

        [Test]
        public void IncreaseTheQualityOnAcceptablePerformance()
        {
            component.currentQualityIndex = 2;
            //component.evaluator = Substitute.For<IAutoQualitySettingsEvaluator>();
            //component.evaluator.Evaluate(null).ReturnsForAnyArgs( 1);

            //component.EvaluateQuality();

            Assert.AreEqual(3, component.currentQualityIndex);
        }
    }
}