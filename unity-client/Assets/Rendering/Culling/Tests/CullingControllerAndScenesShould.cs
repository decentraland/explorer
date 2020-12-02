using System.Collections;
using System.Linq;
using DCL;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace CullingControllerTests
{
    public class CullingControllerAndScenesShould : TestsBase
    {
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            var settings = Environment.i.cullingController.GetSettingsCopy();
            settings.maxTimeBudget = 99999;
            Environment.i.cullingController.SetSettings(settings);
            Environment.i.cullingController.Start();
        }


        [UnityTest]
        public IEnumerator CullMovingEntities()
        {
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, Vector3.one * 1000, true);
            var entity = boxShape.attachedEntities.First();

            Assert.IsTrue(Environment.i.cullingController.IsDirty());

            yield return
                new DCL.WaitUntil(() => entity.meshesInfo.renderers[0].forceRenderingOff, 0.1f);

            Assert.IsTrue(entity.meshesInfo.renderers[0].forceRenderingOff);

            TestHelpers.SetEntityTransform(scene, entity, Vector3.zero, Quaternion.identity, Vector3.one);

            yield return
                new DCL.WaitUntil(() => !entity.meshesInfo.renderers[0].forceRenderingOff, 0.1f);

            Assert.IsFalse(entity.meshesInfo.renderers[0].forceRenderingOff);
        }
    }
}