using NUnit.Framework;
using UnityEngine;
using KernelConfigurationTypes;

public class KernelConfigurationShould
{
    [Test]
    public void TriggerInitializePromiseCorrectly()
    {
        const float testValue = 1234;

        var loadedConfigSO = Resources.Load<KernelConfig>("KernelConfiguration");
        var promiseFromLoaded = loadedConfigSO.EnsureConfigInitialized();
        Assert.IsTrue(promiseFromLoaded.keepWaiting, "Promise shouldn't be resolved until first value is set");

        var promiseFromController = KernelConfigController.GetConfig().EnsureConfigInitialized();
        Assert.IsTrue(promiseFromController.keepWaiting, "Promise shouldn't be resolved until first value is set");

        KernelConfigController.SetConfig(new KernelConfigModel() { comms = new Comms() { commRadius = testValue } });

        Assert.IsFalse(promiseFromController.keepWaiting, "Promise should be resolved");
        Assert.IsFalse(promiseFromLoaded.keepWaiting, "Promise should be resolved");

        Assert.AreEqual(testValue, promiseFromController.value?.comms.commRadius, "Promise value should match configs value");
        Assert.AreEqual(testValue, promiseFromLoaded.value?.comms.commRadius, "Promise value should match configs value");

        var secondLoadedConfigSO = Resources.Load<KernelConfig>("KernelConfiguration");
        var promiseFromSecondLoaded = secondLoadedConfigSO.EnsureConfigInitialized();

        Assert.IsFalse(promiseFromSecondLoaded.keepWaiting, "Promise should be resolved");
        Assert.AreEqual(testValue, promiseFromSecondLoaded.value?.comms.commRadius, "Promise value should match configs value");

        bool promiseFromLoadedPass = false;
        bool promiseFromControllerPass = false;
        bool promiseFromSecondLoadedPass = false;

        promiseFromLoaded.Then((config) => promiseFromLoadedPass = config.comms.commRadius == testValue);
        promiseFromController.Then((config) => promiseFromControllerPass = config.comms.commRadius == testValue);
        promiseFromSecondLoaded.Then((config) => promiseFromSecondLoadedPass = config.comms.commRadius == testValue);

        Assert.IsTrue(promiseFromLoadedPass);
        Assert.IsTrue(promiseFromControllerPass);
        Assert.IsTrue(promiseFromSecondLoadedPass);


        Resources.UnloadAsset(loadedConfigSO);
        Resources.UnloadAsset(secondLoadedConfigSO);
        Resources.UnloadAsset(KernelConfigController.GetConfig());
    }

    [Test]
    public void TriggerOnChangeCorrectly()
    {
        const float testValue = 1234;
        const float testValue2 = 5678;

        bool onChange1Called = false;
        bool onChange2Called = false;
        bool onChange1Pass = false;
        bool onChange2Pass = false;

        KernelConfigModel model = new KernelConfigModel() { comms = new Comms() { commRadius = testValue } };

        KernelConfig.OnKernelConfigChanged onConfigChage1 = (current, prev) =>
        {
            onChange1Called = true;
            onChange1Pass = current.comms.commRadius == testValue;
        };
        KernelConfig.OnKernelConfigChanged onConfigChage2 = (current, prev) =>
        {
            onChange2Called = true;
            onChange2Pass = current.comms.commRadius == testValue;
        };

        var loadedConfigSO = Resources.Load<KernelConfig>("KernelConfiguration");
        loadedConfigSO.OnChange += onConfigChage1;
        KernelConfigController.GetConfig().OnChange += onConfigChage2;

        KernelConfigController.SetConfig(model);
        Assert.IsTrue(onChange1Pass);
        Assert.IsTrue(onChange2Pass);

        onChange1Called = false;
        onChange2Called = false;
        onChange1Pass = false;
        onChange2Pass = false;

        KernelConfigModel modelUpdateWithSameValues = model.Clone();
        KernelConfigController.SetConfig(modelUpdateWithSameValues); // this should trigger onChange cause it has the same values
        Assert.IsFalse(onChange1Called, "OnChange was called even if the new value is equal to the new one");
        Assert.IsFalse(onChange2Called, "OnChange was called even if the new value is equal to the new one");

        loadedConfigSO.OnChange -= onConfigChage1;
        KernelConfigController.GetConfig().OnChange -= onConfigChage2;

        onConfigChage1 = (current, prev) =>
        {
            onChange1Called = true;
            onChange1Pass = current.comms.commRadius == testValue2 && prev.comms.commRadius == testValue;
        };
        onConfigChage2 = (current, prev) =>
        {
            onChange2Called = true;
            onChange2Pass = current.comms.commRadius == testValue2 && prev.comms.commRadius == testValue;
        };

        loadedConfigSO.OnChange += onConfigChage1;
        KernelConfigController.GetConfig().OnChange += onConfigChage2;

        KernelConfigController.SetConfig(new KernelConfigModel() { comms = new Comms() { commRadius = testValue2 } });
        Assert.IsTrue(onChange1Pass);
        Assert.IsTrue(onChange2Pass);

        loadedConfigSO.OnChange -= onConfigChage1;
        KernelConfigController.GetConfig().OnChange -= onConfigChage2;

        Resources.UnloadAsset(loadedConfigSO);
        Resources.UnloadAsset(KernelConfigController.GetConfig());
    }

    [Test]
    public void ParseJsonCorrectly()
    {
        KernelConfigModel model = new KernelConfigModel();
        string json = JsonUtility.ToJson(model);
        KernelConfigController.SetConfig(json);

        Assert.IsTrue(model.Equals(KernelConfigController.GetConfig().Get()));

        Resources.UnloadAsset(KernelConfigController.GetConfig());
    }
}
