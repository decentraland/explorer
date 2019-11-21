using DCL;
using NUnit.Framework;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class UtilsTests
{
    [Test]
    public void CIDtoMD5Test()
    {
        Assert.AreEqual("d3b55cc7e3367537c1670ecebb1ccb05", DCL.AssetBundleBuilderUtils.GetGUID("QmWVcyTEzSEBKC7hzq6doiTWnopZ6DdqJMqufx6gXwFnTS"));
    }

    [UnityTest]
    public IEnumerator EvaluateDependency()
    {
        Caching.ClearCache();

        if (Directory.Exists(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT))
            Directory.Delete(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT, true);

        if (Directory.Exists(AssetBundleBuilderConfig.DOWNLOADED_PATH_ROOT))
            Directory.Delete(AssetBundleBuilderConfig.DOWNLOADED_PATH_ROOT, true);

        AssetDatabase.Refresh();

        var builder = new AssetBundleBuilder();
        bool finished = false;

        System.Action<AssetBundleBuilder.ErrorCodes> onFinish = (x) => { finished = true; };

        builder.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1), onFinish);

        yield return new WaitUntil(() => finished == true);

        EvaluateDependencyAfterBuild();
    }

    void EvaluateDependencyAfterBuild()
    {
        AssetBundle abDependency = AssetBundle.LoadFromFile(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT + "/QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5");
        abDependency.LoadAllAssets();

        AssetBundle abMain = AssetBundle.LoadFromFile(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT + "/QmNS4K7GaH63T9rhAfkrra7ADLXSEeco8FTGknkPnAVmKM");
        Material[] mats = abMain.LoadAllAssets<Material>();

        bool hasMap = false;

        foreach (var mat in mats)
        {
            if (mat.name.ToLowerInvariant().Contains("mini town"))
                hasMap = mat.GetTexture("_BaseMap") != null;
        }

        abMain.Unload(true);
        abDependency.Unload(true);

        if (hasMap)
        {
            Debug.Log("Dependency has been generated correctly!");
        }
        else
        {
            Debug.Log("Dependency has NOT been generated correctly!");
        }

        Assert.IsTrue(hasMap);
    }

}
