using System.Collections;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Camera_Test
{
    public class CameraControllerShould : TestsBase
    {
        private CameraController cameraController;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return InitScene(spawnCharController: true);
            cameraController = Object.FindObjectOfType<CameraController>(); //Camera controller should be in the CharacterController
        }

        [Test]
        public void InitializeCameraSetupsCorrectly()
        {
            Assert.IsTrue(cameraController.cameraSetups.ContainsKey(CameraController.CameraState.FirstPerson));
            Assert.IsNotNull(cameraController.cameraSetups[CameraController.CameraState.FirstPerson]);
            Assert.IsTrue(cameraController.cameraSetups[CameraController.CameraState.FirstPerson] is FirstPersonCameraSetup);

            Assert.IsTrue(cameraController.cameraSetups.ContainsKey(CameraController.CameraState.ThirdPerson));
            Assert.IsNotNull(cameraController.cameraSetups[CameraController.CameraState.ThirdPerson]);
            Assert.IsTrue(cameraController.cameraSetups[CameraController.CameraState.ThirdPerson] is ThirdPersonCameraSetup);
        }

        [Test]
        public void ReactToCameraStateChanges()
        {
            var cameraStateSO = cameraController.currentState;

            cameraStateSO.Set(CameraController.CameraState.FirstPerson);
            Assert.IsTrue(cameraController.currentSetup is FirstPersonCameraSetup);

            cameraStateSO.Set(CameraController.CameraState.ThirdPerson);
            Assert.IsTrue(cameraController.currentSetup is ThirdPersonCameraSetup);
        }
    }

    public class CameraSetupFactoryShould : TestsBase
    {
        [Test]
        public void CreateFirstPersonSetupCorrectly()
        {
            var config = ScriptableObject.CreateInstance<FirstPersonCameraConfigSO>();
            var dummyCamera = new GameObject("_dummyCamera").AddComponent<Camera>();

            var cameraSetup = (FirstPersonCameraSetup)CameraSetupFactory.CreateCameraSetup(CameraController.CameraState.FirstPerson, dummyCamera, config);

            Assert.AreEqual(config, cameraSetup.configuration);
            Assert.AreEqual(dummyCamera, cameraSetup.camera);
        }
        
        [Test]
        public void CreateThirdPersonSetupCorrectly()
        {
            var config = ScriptableObject.CreateInstance<ThirdPersonCameraConfigSO>();
            var dummyCamera = new GameObject("_dummyCamera").AddComponent<Camera>();

            var cameraSetup = (ThirdPersonCameraSetup)CameraSetupFactory.CreateCameraSetup(CameraController.CameraState.ThirdPerson, dummyCamera, config);

            Assert.AreEqual(config, cameraSetup.configuration);
            Assert.AreEqual(dummyCamera, cameraSetup.camera);
        }
    }

    public class FirstPersonCameraShould : TestsBase
    {
        private FirstPersonCameraConfigSO config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return InitScene();
            config = ScriptableObject.CreateInstance<FirstPersonCameraConfigSO>();
        }

        [Test]
        public void NotModifyTheTransformWithoutActivation()
        {
            config.Set(new FirstPersonCameraConfig() { yOffset = 10 });
            var dummyCamera = new GameObject("_dummyCamera").AddComponent<Camera>();
            dummyCamera.transform.position = Vector3.up * 1.5f;

            var cameraSetup = new FirstPersonCameraSetup(dummyCamera, config);

            Assert.AreEqual(Vector3.up * 1.5f, dummyCamera.transform.position);
        }

        [Test]
        public void ModifyTheTransformOnActivation()
        {
            config.Set(new FirstPersonCameraConfig() { yOffset = 10 });
            var dummyCamera = new GameObject("_dummyCamera").AddComponent<Camera>();

            var cameraSetup = new FirstPersonCameraSetup(dummyCamera, config);
            cameraSetup.Activate();

            Assert.AreEqual(config.Get().yOffset, dummyCamera.transform.position.y);
        }

        [Test]
        public void ReactToChangesInConfig()
        {
            config.Set(new FirstPersonCameraConfig() { yOffset = 10 });
            var dummyCamera = new GameObject("_dummyCamera").AddComponent<Camera>();
            var cameraSetup = new FirstPersonCameraSetup(dummyCamera, config);
            cameraSetup.Activate();

            config.Set(new FirstPersonCameraConfig() { yOffset = 77 });

            Assert.AreEqual(config.Get().yOffset, dummyCamera.transform.position.y);
        }
    }

    public class ThirdPersonCameraShould : TestsBase
    {
        private ThirdPersonCameraConfigSO config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return InitScene();
            config = ScriptableObject.CreateInstance<ThirdPersonCameraConfigSO>();
        }

        [Test]
        public void NotModifyTheTransformWithoutActivation()
        {
            config.Set(new ThirdPersonCameraConfig() { offset = Vector3.one * 3 });
            var dummyCamera = new GameObject("_dummyCamera").AddComponent<Camera>();
            dummyCamera.transform.position = Vector3.up * 1.5f;

            var cameraSetup = new ThirdPersonCameraSetup(dummyCamera, config);

            Assert.AreNotEqual(config.Get().offset, dummyCamera.transform.position);
        }

        [Test]
        public void ModifyTheTransformOnActivation()
        {
            config.Set(new ThirdPersonCameraConfig() { offset = Vector3.one * 3 });
            var dummyCamera = new GameObject("_dummyCamera").AddComponent<Camera>();

            var cameraSetup = new ThirdPersonCameraSetup(dummyCamera, config);
            cameraSetup.Activate();

            Assert.AreEqual(config.Get().offset, dummyCamera.transform.position);
        }

        [Test]
        public void ReactToChangesInConfig()
        {
            config.Set(new ThirdPersonCameraConfig() { offset = Vector3.one * 3 });
            var dummyCamera = new GameObject("_dummyCamera").AddComponent<Camera>();
            var cameraSetup = new ThirdPersonCameraSetup(dummyCamera, config);
            cameraSetup.Activate();

            config.Set(new ThirdPersonCameraConfig() { offset = Vector3.one * 6 });

            Assert.AreEqual(config.Get().offset, dummyCamera.transform.position);
        }
    }
}