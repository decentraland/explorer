using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

public class GLTFImporterTests : TestsBase
{
    public IEnumerator LoadModel(string path, System.Action<InstantiatedGLTFObject> OnFinishLoading)
    {
        string src = Utils.GetTestsAssetsPath() + path;
        DecentralandEntity entity = null;
        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, src, out entity);
        yield return gltfShape.routine;
        yield return new WaitForSeconds(4);

        if (OnFinishLoading != null)
        {
            OnFinishLoading.Invoke(entity.meshRootGameObject.GetComponentInChildren<InstantiatedGLTFObject>());
        }
    }

    [UnityTest]
    [Explicit("Test takes too long")]
    [Category("Explicit")]
    public IEnumerator TrevorModelHasProperScaling()
    {
        InstantiatedGLTFObject trevorModel = null;
        yield return LoadModel("/GLB/Trevor/Trevor.glb", (m) => trevorModel = m);

        Transform child = trevorModel.transform.GetChild(0).GetChild(0);
        Vector3 scale = child.lossyScale;
        Assert.AreEqual(new Vector3(0.049f, 0.049f, 0.049f).ToString(), scale.ToString());
        yield return null;
    }

    [UnityTest]
    [Explicit("Test takes too long")]
    [Category("Explicit")]
    public IEnumerator TrevorModelHasProperTopology()
    {
        InstantiatedGLTFObject trevorModel = null;
        yield return LoadModel("/GLB/Trevor/Trevor.glb", (m) => trevorModel = m);

        Assert.IsTrue(trevorModel.transform.childCount == 1);
        Assert.IsTrue(trevorModel.transform.GetChild(0).childCount == 2);
        Assert.IsTrue(trevorModel.transform.GetChild(0).GetChild(0).name.Contains("Character_Avatar"));
        Assert.IsTrue(trevorModel.transform.GetChild(0).GetChild(1).name.Contains("mixamorig"));
        yield return null;
    }

    [UnityTest]
    [Explicit("Test takes too long")]
    [Category("Explicit")]
    public IEnumerator GLTFWithoutSkeletonIdIsLoadingCorrectly()
    {
        InstantiatedGLTFObject trevorModel = null;
        yield return LoadModel("/GLB/Avatar/Avatar_Idle.glb", (m) => trevorModel = m);
    }

    [UnityTest]
    public IEnumerator CurvesAreOptimizedCorrectly()
    {
        var curveContainer = Resources.Load<AnimationCurveContainer>("AnimationCurveContainer");

        for (int i = 0; i < curveContainer.curves.Length; i++)
        {
            var curve = curveContainer.curves[i];

            List<Keyframe> keys = new List<Keyframe>();

            for (int i1 = 0; i1 < curve.length; i1++)
            {
                keys.Add(curve[i1]);
            }

            var result = GLTFSceneImporter.OptimizeKeyFrames(keys.ToArray());

            var modifiedCurve = new AnimationCurve(result);

            curveContainer.curves[i] = modifiedCurve;

            for (float time = 0; time < 1.0f; time += 0.032f)
            {
                var v1 = curve.Evaluate(time);
                var v2 = modifiedCurve.Evaluate(time);

                UnityEngine.Assertions.Assert.AreApproximatelyEqual(v1, v2, 0.2f);
            }
        }

        yield break;
    }
}