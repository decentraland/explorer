using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace SceneBoundariesCheckerTests
{

    public class SceneBoundariesCheckerTests : TestsBase
    {
        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Too slow")]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene);
        }



        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene);
        }



        [UnityTest]
        [Category("Explicit")]
        [Explicit("Too slow")]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenLeavingBounds(scene);
        }

        [UnityTest]
        public IEnumerator PShapeIsResetWhenReenteringBounds()
        {
            yield return SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene);
        }



        [UnityTest]
        public IEnumerator GLTFShapeIsResetWhenReenteringBounds()
        {
            yield return SBC_Asserts.GLTFShapeIsResetWhenReenteringBounds(scene);
        }




        [UnityTest]
        [Category("Explicit")]
        [Explicit("Too slow")]
        public IEnumerator NFTShapeIsResetWhenReenteringBounds()
        {
            yield return SBC_Asserts.NFTShapeIsResetWhenReenteringBounds(scene);
        }



        [UnityTest]
        public IEnumerator ChildShapeIsEvaluated()
        {
            yield return SBC_Asserts.ChildShapeIsEvaluated(scene);
        }


        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParent()
        {
            yield return SBC_Asserts.ChildShapeIsEvaluatedOnShapelessParent(scene);
        }

        [UnityTest]
        public IEnumerator HeightIsEvaluated()
        {
            yield return SBC_Asserts.HeightIsEvaluated(scene);
        }



        public bool MeshIsInvalid(DecentralandEntity.MeshesInfo meshesInfo)
        {
            return SBC_Asserts.MeshIsInvalid(meshesInfo);
        }

    }
}
