using DCL;
using DCL.Helpers;
using DCL.Components;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using DCL.Controllers;

namespace Tests
{
    public class VideoTests : TestsBase
    {

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            DCLVideoTexture.isTest = true;
        }

        [UnityTest]
        public IEnumerator VideoTextureIsAttachedAndDetachedCorrectly()
        {
            // TEST: DCLVideoTexture is created correctly
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, "DCLVideoTexture started with attachedMaterials != 0");

            // TEST: DCLVideoTexture replace another texture in material
            DCLTexture dclTexture = TestHelpers.CreateDCLTexture(
                scene,
                Utils.GetTestsAssetsPath() + "/Images/atlas.png",
                DCLTexture.BabylonWrapMode.CLAMP,
                FilterMode.Bilinear);

            yield return dclTexture.routine;

            BasicMaterial mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>
            (scene, CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model
                {
                    texture = dclTexture.id
                });

            yield return mat.routine;

            yield return TestHelpers.SharedComponentUpdate<BasicMaterial, BasicMaterial.Model>(mat, new BasicMaterial.Model() { texture = videoTexture.id });

            Assert.IsTrue(videoTexture.attachedMaterials.Count == 1, $"did DCLVideoTexture attach to material? {videoTexture.attachedMaterials.Count} expected 1");

            // TEST: DCLVideoTexture added to fresh material
            BasicMaterial mat2 = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>
            (scene, CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model
                {
                    texture = videoTexture.id
                });

            yield return mat2.routine;

            Assert.IsTrue(videoTexture.attachedMaterials.Count == 2, $"did DCLVideoTexture attach to material? {videoTexture.attachedMaterials.Count} expected 2");

            // TEST: DCLVideoTexture detach on material disposed
            mat2.Dispose();
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 1, $"did DCLVideoTexture detach from material? {videoTexture.attachedMaterials.Count} expected 1");
            mat.Dispose();
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, $"did DCLVideoTexture detach from material? {videoTexture.attachedMaterials.Count} expected 0");

            videoTexture.Dispose();

            yield return null;
            Assert.IsTrue(videoTexture.texture == null, "DCLVideoTexture didn't dispose correctly?");
        }

        [UnityTest]
        public IEnumerator VideoTextureVisibleStateIsSetCorrectly()
        {
            //NOTE: DCLVideoTexture is not visible if there is no shape
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            DecentralandEntity ent1 = TestHelpers.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestHelpers.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible without a shape");

            BoxShape ent1Shape = TestHelpers.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestHelpers.SharedComponentAttach(ent1Shape, ent1);
            yield return null; //a frame to wait DCLVideoTexture update

            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            //NOTE: DCLVideoTexture should be visible when a shape is visible and the other shape is insivible and both shape share material
            DecentralandEntity ent2 = TestHelpers.CreateSceneEntity(scene);
            BoxShape ent2Shape = TestHelpers.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent2Shape.routine;
            TestHelpers.SharedComponentAttach(ent2Shape, ent2);
            yield return null; //a frame to wait DCLVideoTexture update

            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent2Shape, new BoxShape.Model() { visible = false });
            yield return null; //a frame to wait DCLVideoTexture update
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            //NOTE: DCLVideoTexture should be invisible when both shapes are invisible
            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent1Shape, new BoxShape.Model() { visible = false });
            yield return null; //a frame to wait DCLVideoTexture update
            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible");

            //NOTE: DCLVideoTexture should become visible again when both shapes are visible
            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent2Shape, new BoxShape.Model() { visible = true });
            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent1Shape, new BoxShape.Model() { visible = true });
            yield return null; //a frame to wait DCLVideoTexture update
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            //NOTE: DCLVideoTexture should stay visible if only one entity change it material
            BasicMaterial ent2Mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model());
            TestHelpers.SharedComponentAttach(ent2Mat, ent2);
            yield return ent2Mat.routine;
            yield return null; //a frame to wait DCLVideoTexture update
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            //NOTE: DCLVideoTexture should be invisible when the other shape is set to invisible
            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent1Shape, new BoxShape.Model() { visible = false });
            yield return null; //a frame to wait DCLVideoTexture update
            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible");

            //NOTE: DCLVideoTexture should be visible when shapes is set to visible again and it is changed      
            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent1Shape, new BoxShape.Model() { visible = true });
            yield return null; //a frame to wait DCLVideoTexture update
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            BoxShape ent3Shape = TestHelpers.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent3Shape.routine;
            TestHelpers.SharedComponentAttach(ent3Shape, ent1);
            yield return null; //a frame to wait DCLVideoTexture update
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            //NOTE: DCLVideoTexture should stay visible when adding a new material using the same texture and hidding previous shape
            BasicMaterial ent3Mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestHelpers.SharedComponentAttach(ent1Mat, ent2);
            yield return ent1Mat.routine;
            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent3Shape, new BoxShape.Model() { visible = false });
            yield return null; //a frame to wait DCLVideoTexture update
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent2Shape, new BoxShape.Model() { visible = false });
            yield return null; //a frame to wait DCLVideoTexture update
            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible");

        }

        static DCLVideoClip CreateDCLVideoClip(ParcelScene scn, string url)
        {
            return TestHelpers.SharedComponentCreate<DCLVideoClip, DCLVideoClip.Model>
            (
                scn,
                DCL.Models.CLASS_ID.VIDEO_CLIP,
                new DCLVideoClip.Model
                {
                    url = url
                }
            );
        }

        static DCLVideoTexture CreateDCLVideoTexture(ParcelScene scn, DCLVideoClip clip)
        {
            return TestHelpers.SharedComponentCreate<DCLVideoTexture, DCLVideoTexture.Model>
            (
                scn,
                DCL.Models.CLASS_ID.VIDEO_TEXTURE,
                new DCLVideoTexture.Model
                {
                    videoClipId = clip.id
                }
            );
        }

        static DCLVideoTexture CreateDCLVideoTexture(ParcelScene scn, string url)
        {
            return CreateDCLVideoTexture(scn, CreateDCLVideoClip(scn, url));
        }
    }
}