using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class SectionDeployedScenesViewShould
    {
        private SectionDeployedScenesView view;

        [SetUp]
        public void SetUp()
        {
            const string prefabAssetPath = "BuilderProjectsPanelMenuSections/SectionDeployedScenesView";
            var prefab = Resources.Load<SectionDeployedScenesView>(prefabAssetPath);
            view = Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(view.gameObject);
        }

        [Test]
        public void HaveScenesContainerEmptyAtInstantiation()
        {
            Assert.AreEqual(0, view.scenesCardContainer.childCount);
        }
    }
}