using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class SectionProjectScenesViewShould
    {
        private SectionProjectScenesView view;

        [SetUp]
        public void SetUp()
        {
            const string prefabAssetPath = "BuilderProjectsPanelMenuSections/SectionProjectScenesView";
            var prefab = Resources.Load<SectionProjectScenesView>(prefabAssetPath);
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