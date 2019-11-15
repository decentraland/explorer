using DCL;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using MappingPair = DCL.ContentServerUtils.MappingPair;

public class AssetBundleIntegrationTest
{

    static readonly string PINE_TEXTURE = "models/TreeRoundPine_02/file1.png";
    static readonly string PINE_TEXTURE_HASH = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
    static readonly string PINE_FILE = "models/TreeRoundPine_02/TreeRoundPine_02.glb";
    static readonly string PINE_HASH = "QmdVoQrhTkRycqEZBWcTdCuC7xwEeEjqS798SxAo1dmEUT";
    static readonly string DCL_FILE = "models/DecentralandLogo_01/DecentralandLogo_01.glb";
    static readonly string DCL_HASH = "QmVQYjtHr399TUXApdnPWBwT5Y4xct2TyJimj2dFtSTqj1";
    static readonly string DCL_TEXTURE_FILE = "models/DecentralandLogo_01/file1.png";
    static readonly string DCL_TEXTURE_HASH = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
    static readonly string TREE_FILE = "models/FloorBaseWood_01/FloorBaseWood_01.glb";
    static readonly string TREE_HASH = "QmYDDgRJUTNrAwVhNnWowoyKCC2sXszFcGNwu8qbV7kEWg";
    static readonly string TREE_TEXTURE_FILE = "models/FloorBaseWood_01/Floor_Wood.png.001.png";
    static readonly string TREE_TEXTURE_HASH = "QmQMtLv8RASpvW4E2GwowtwtnMW7CNMxU4VEudCvxKufdn";

    static readonly MappingPair PineTexture = new MappingPair(PINE_TEXTURE, PINE_TEXTURE_HASH);
    static readonly MappingPair PineModel = new MappingPair(PINE_FILE, PINE_HASH);
    static readonly MappingPair DCLTexture = new MappingPair(DCL_TEXTURE_FILE, DCL_TEXTURE_HASH);
    static readonly MappingPair DCLModel = new MappingPair(DCL_FILE, DCL_HASH);
    static readonly MappingPair TreeTexture = new MappingPair(TREE_TEXTURE_FILE, TREE_TEXTURE_HASH);
    static readonly MappingPair TreeModel = new MappingPair(TREE_FILE, TREE_HASH);

    static readonly ContentProvider PineContent = new ContentProvider("https://content.decentraland.org/contents/", new List<MappingPair>() { PineModel, PineTexture });
    static readonly ContentProvider MultipleTextureContent = new ContentProvider(
        "https://content.decentraland.org/contents/",
        new List<MappingPair>() { PineModel, PineTexture, DCLModel, DCLTexture, TreeModel, TreeTexture }
    );

    [SetUp]
    public void SetupTemporaryFolders()
    {
        BundleBuilder.InitializeFilesystemFolders();
    }

    [Test]
    public void GenerateTextureAssetBundles()
    {
        BundleBuilder.DownloadRawContent(PineContent, PINE_TEXTURE);
        var assetBundlePath = BundleBuilder.GenerateTextureAssetBundle(PineContent, PINE_TEXTURE);

        Assert.AreEqual(assetBundlePath, PINE_TEXTURE_HASH.ToLowerInvariant());
        FileAssert.Exists(Path.Combine(BundleBuilder.ASSET_BUNDLE_OUTPUT_FOLDER, assetBundlePath));
    }

    [Test]
    public void GenerateMultipleTextureAssetBundles()
    {
        var assetBundlePaths = BundleBuilder.GenerateTextureAssetBundles(MultipleTextureContent);

        var equalArrays = (assetBundlePaths[0].Equals(TREE_TEXTURE_HASH.ToLowerInvariant())
            || assetBundlePaths[0].Equals(DCL_TEXTURE_HASH.ToLowerInvariant())) &&
            (assetBundlePaths[1].Equals(TREE_TEXTURE_HASH.ToLowerInvariant())
            || assetBundlePaths[1].Equals(DCL_TEXTURE_HASH.ToLowerInvariant()));
        Assert.IsTrue(equalArrays);
        FileAssert.Exists(Path.Combine(BundleBuilder.ASSET_BUNDLE_OUTPUT_FOLDER, DCL_TEXTURE_HASH.ToLowerInvariant()));
        FileAssert.Exists(Path.Combine(BundleBuilder.ASSET_BUNDLE_OUTPUT_FOLDER, TREE_TEXTURE_HASH.ToLowerInvariant()));
    }

    [Test]
    public void GenerateGLTFAssetBundle()
    {
        BundleBuilder.GenerateTextureAssetBundles(MultipleTextureContent);
        BundleBuilder.CleanupWorkingDir();
        BundleBuilder.InitializeFilesystemFolders();
        BundleBuilder.GenerateGLTFAssetBundle(PineContent, PINE_FILE);
        FileAssert.Exists(Path.Combine(BundleBuilder.ASSET_BUNDLE_OUTPUT_FOLDER, PINE_HASH.ToLowerInvariant()));
    }

    [Test]
    public void EnsureTexturesOnAssetBundle()
    {
    }

    [Test]
    public void EnsureGUIDsAreCorrectlyReferenced()
    {
        var assetBundle = BundleBuilder.GenerateTextureAssetBundle(PineContent, PINE_TEXTURE);
        BundleBuilder.CleanupWorkingDir();
        BundleBuilder.InitializeFilesystemFolders();
        File.Copy(
            Path.Combine(BundleBuilder.ASSET_BUNDLE_OUTPUT_FOLDER, assetBundle + ".manifest"),
            Path.Combine(BundleBuilder.ASSET_BUNDLE_RELATIVE_WORKING_DIR, assetBundle + ".manifest")
        );
        File.Copy(
            Path.Combine(BundleBuilder.ASSET_BUNDLE_OUTPUT_FOLDER, assetBundle),
            Path.Combine(BundleBuilder.ASSET_BUNDLE_RELATIVE_WORKING_DIR, assetBundle)
        );
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(
            Path.Combine(BundleBuilder.ASSET_BUNDLE_RELATIVE_WORKING_DIR, assetBundle + ".manifest")
        );
        AssetDatabase.Refresh();
        Debug.Log(AssetDatabase.GetAllAssetPaths());
        Debug.Log(AssetDatabase.FindAssets(assetBundle));
    }

    [Test]
    public void DownloadRawContentToTempFolder()
    {
        var downloadedFilePath = BundleBuilder.DownloadRawContent(PineContent, PINE_TEXTURE);

        FileAssert.Exists(downloadedFilePath);
    }

    [Test]
    public void DownloadToWorkingFolder()
    {
        var workingFilePath = BundleBuilder.DownloadIntoWorkingFolder(PineContent, PINE_TEXTURE);

        Assert.AreEqual(
            workingFilePath,
            Path.Combine(
                BundleBuilder.ASSET_BUNDLE_WORKING_DIR,
                PineContent.GetLowercaseHashWithExtension(PINE_TEXTURE)
            )
        );
        FileAssert.Exists(workingFilePath);
    }

    [Test]
    public void FilterWithExtension()
    {
        var contents = BundleBuilder.FilterOnlyAssetsWithExtension(PineContent, new string[] { "png" });

        Assert.AreEqual(contents, new string[] { PINE_TEXTURE });
    }

    [Test]
    public void RelativePathIsRelative()
    {
        var path = BundleBuilder.GetRelativeWorkingPathForFile(PineContent, PINE_FILE);
        Assert.IsTrue(path.StartsWith(BundleBuilder.ASSET_BUNDLE_RELATIVE_WORKING_DIR, System.StringComparison.Ordinal));
    }

    [TearDown]
    public void RemoveTemporaryFolders()
    {
        BundleBuilder.CleanupWorkingDir();
    }
}
