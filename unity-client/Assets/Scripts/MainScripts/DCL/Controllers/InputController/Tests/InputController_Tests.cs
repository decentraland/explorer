using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace InputController_Tests
{
    public class InputAction_OneTime_Should
    {
        private InputAction_OneTime action;

        [SetUp]
        public void SetUp()
        {
            action = ScriptableObject.CreateInstance<InputAction_OneTime>();
        }

        [Test]
        public void Have_WasTriggeredThisFrame_Off_ByDefault()
        {
            Assert.IsFalse(action.WasTriggeredThisFrame());
        }

        [Test]
        public void Have_WasTriggeredThisFrame_On_AfterRaisingInTheSameFrame()
        {
            action.RaiseOnTriggered();
            Assert.True(action.WasTriggeredThisFrame());
        }

        [UnityTest]
        public IEnumerator Have_WasTriggeredThisFrame_Off_AfterRaisingInDifferentFrame()
        {
            action.RaiseOnTriggered();
            yield return null;
            Assert.False(action.WasTriggeredThisFrame());
        }

        [Test]
        public void CallTriggeredEvent()
        {
            bool called = false;
            action.OnTriggered += x => called = true;

            action.RaiseOnTriggered();

            Assert.IsTrue(called);
        }

        [Test]
        public void ReturnDCLActionProperly()
        {
            action.dclAction = DCLAction_OneTime.Jump;

            Assert.AreEqual(DCLAction_OneTime.Jump, action.GetDCLAction());
        }
    }

    public class InputAction_InTime_Should
    {
        private InputAction_InTime action;

        [SetUp]
        public void SetUp()
        {
            action = ScriptableObject.CreateInstance<InputAction_InTime>();
        }

        [Test]
        public void BeOffByDefault()
        {
            Assert.IsFalse(action.isOn);
        }

        [Test]
        public void CallStartedEvent()
        {
            bool called = false;
            action.OnStarted += x => called = true;

            action.RaiseOnStarted();

            Assert.IsTrue(called);
        }

        [Test]
        public void CallFinishedEvent()
        {
            bool called = false;
            action.OnFinished += x => called = true;

            action.RaiseOnFinished();

            Assert.IsTrue(called);
        }

        [Test]
        public void BeOnAfterStartedIsRaised()
        {
            action.RaiseOnStarted();
            Assert.IsTrue(action.isOn);
        }

        [Test]
        public void BeOffAfterFinishedIsRaised()
        {
            action.RaiseOnStarted();
            action.RaiseOnFinished();
            Assert.IsFalse(action.isOn);
        }

        [Test]
        public void ReturnDCLActionProperly()
        {
            action.dclAction = DCLAction_InTime.Sprint;

            Assert.AreEqual(DCLAction_InTime.Sprint, action.GetDCLAction());
        }
    }

    public class InputAction_Measurable_Should
    {
        private InputAction_Measurable action;

        [SetUp]
        public void SetUp()
        {
            action = ScriptableObject.CreateInstance<InputAction_Measurable>();
        }

        [Test]
        public void Return_0_AsDefaultValue()
        {
            Assert.AreEqual(0, action.GetValue());
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(-1)]
        [TestCase(0.5f)]
        public void UpdateValueProperly(float newValue)
        {
            action.RaiseOnValueChanged(newValue);
            Assert.AreEqual(newValue, action.GetValue());
        }

        [Test]
        public void CallOnValueChangedEvent()
        {
            var called = false;
            var calledValue = 18f;

            action.OnValueChanged += (x, value) =>
            {
                called = true;
                calledValue = value;
            };
            action.RaiseOnValueChanged(18f);

            Assert.IsTrue(called);
            Assert.AreEqual(18f, calledValue);
        }

        [Test]
        public void ReturnDCLActionProperly()
        {
            action.dclAction = DCLAction_Measurable.CameraXAxis;

            Assert.AreEqual(DCLAction_Measurable.CameraXAxis, action.GetDCLAction());
        }
    }
}