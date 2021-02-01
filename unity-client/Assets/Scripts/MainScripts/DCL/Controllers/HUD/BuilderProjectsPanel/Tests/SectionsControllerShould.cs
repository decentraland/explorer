using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class SectionsControllerShould
    {
        private SectionsController controller;
        private SectionFactory sectionFactory;

        [SetUp]
        public void SetUp()
        {
            sectionFactory = new SectionFactory();
            controller = new SectionsController(sectionFactory, null);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void OpenSection()
        {
            bool openCallbackCalled = false;
            SectionBase sectionOpened = null;

            void OnSectionOpen(SectionBase section)
            {
                openCallbackCalled = true;
                sectionOpened = section;
            }

            controller.OnSectionShow += OnSectionOpen;
            controller.OpenSection(SectionsController.SectionId.SCENES_MAIN);

            Assert.IsTrue(openCallbackCalled);
            Assert.IsTrue(sectionFactory.sectionScenesMain.isVisible);
            Assert.AreEqual(sectionFactory.sectionScenesMain, sectionOpened);
        }

        [Test]
        public void SwitchOpenSection()
        {
            SectionBase openSection = null;
            SectionBase hiddenSection = null;

            void OnSectionOpen(SectionBase section)
            {
                openSection = section;
            }

            void OnSectionHide(SectionBase section)
            {
                hiddenSection = section;
            }

            controller.OnSectionShow += OnSectionOpen;
            controller.OnSectionHide += OnSectionHide;

            controller.OpenSection(SectionsController.SectionId.SCENES_MAIN);

            Assert.IsTrue(sectionFactory.sectionScenesMain.isVisible);
            Assert.AreEqual(sectionFactory.sectionScenesMain, openSection);

            controller.OpenSection(SectionsController.SectionId.SCENES_PROJECT);

            Assert.IsFalse(sectionFactory.sectionScenesMain.isVisible);
            Assert.IsTrue(sectionFactory.sectionScenesProjects.isVisible);

            Assert.AreEqual(sectionFactory.sectionScenesProjects, openSection);
            Assert.AreEqual(sectionFactory.sectionScenesMain, hiddenSection);
        }
    }

    class SectionFactory : ISectionFactory
    {
        public SectionScenesMain sectionScenesMain;
        public SectionScenesProjects sectionScenesProjects;

        public SectionFactory()
        {
            sectionScenesMain = new SectionScenesMain();
            sectionScenesProjects = new SectionScenesProjects();
        }

        SectionBase ISectionFactory.GetSectionController(SectionsController.SectionId id)
        {
            SectionBase result = null;
            switch (id)
            {
                case SectionsController.SectionId.SCENES_MAIN:
                    result = sectionScenesMain;
                    break;
                case SectionsController.SectionId.SCENES_DEPLOYED:
                    break;
                case SectionsController.SectionId.SCENES_PROJECT:
                    result = sectionScenesProjects;
                    break;
                case SectionsController.SectionId.LAND:
                    break;
            }

            return result;
        }
    }

    class SectionBaseTest : SectionBase
    {
        public override void SetViewContainer(Transform viewContainer)
        {
        }

        public override void Dispose()
        {
        }

        public override void OnShow()
        {
        }

        public override void OnHide()
        {
        }
    }

    class SectionScenesMain : SectionBaseTest
    {
    }

    class SectionScenesProjects : SectionBaseTest
    {
    }
}