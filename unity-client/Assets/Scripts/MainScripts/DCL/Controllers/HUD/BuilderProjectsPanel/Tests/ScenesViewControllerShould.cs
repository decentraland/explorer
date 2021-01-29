using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;

namespace Tests
{
    public class ScenesViewControllerShould
    {
        private ScenesViewController scenesViewController;
        private Listener listener;

        [SetUp]
        public void SetUp()
        {
            const string prefabAssetPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/SceneCardView.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<SceneCardView>(prefabAssetPath);

            scenesViewController = new ScenesViewController(prefab);
            listener = new Listener();

            scenesViewController.OnDeployedSceneRemoved += ((IDeployedSceneListener)listener).OnSceneRemoved;
            scenesViewController.OnDeployedSceneAdded += ((IDeployedSceneListener)listener).OnSceneAdded;
            scenesViewController.OnDeployedScenesSet += ((IDeployedSceneListener)listener).OnSetScenes;

            scenesViewController.OnProjectSceneRemoved += ((IProjectSceneListener)listener).OnSceneRemoved;
            scenesViewController.OnProjectSceneAdded += ((IProjectSceneListener)listener).OnSceneAdded;
            scenesViewController.OnProjectScenesSet += ((IProjectSceneListener)listener).OnSetScenes;
        }

        [TearDown]
        public void TearDown()
        {
            scenesViewController.Dispose();
        }

        [Test]
        public void CallListenerEventsCorrectly()
        {
            scenesViewController.SetScenes(new List<ISceneData>(){new SceneData(){id = "1", isDeployed = true}});

            Assert.AreEqual(1, listener.deployedScenes.Count);
            Assert.AreEqual(1, listener.setScenes.Count);
            Assert.AreEqual(0, listener.addedScenes.Count);
            Assert.AreEqual(0, listener.removedScenes.Count);
            Assert.AreEqual(0, listener.projectScenes.Count);

            listener.Clear();

            scenesViewController.SetScenes(new List<ISceneData>()
            {
                new SceneData(){id = "1", isDeployed = true},
                new SceneData(){id = "2", isDeployed = true}
            });

            Assert.AreEqual(2, listener.deployedScenes.Count);
            Assert.AreEqual(0, listener.setScenes.Count);
            Assert.AreEqual(1, listener.addedScenes.Count);
            Assert.AreEqual(0, listener.removedScenes.Count);
            Assert.AreEqual(0, listener.projectScenes.Count);

            listener.Clear();

            scenesViewController.SetScenes(new List<ISceneData>()
            {
                new SceneData(){id = "1", isDeployed = true},
                new SceneData(){id = "2", isDeployed = false}
            });

            Assert.AreEqual(1, listener.deployedScenes.Count);
            Assert.AreEqual(1, listener.setScenes.Count);
            Assert.AreEqual(0, listener.addedScenes.Count);
            Assert.AreEqual(1, listener.removedScenes.Count);
            Assert.AreEqual(1, listener.projectScenes.Count);

            listener.Clear();

            scenesViewController.SetScenes(new List<ISceneData>()
            {
                new SceneData(){id = "1", isDeployed = true},
                new SceneData(){id = "2", isDeployed = false}
            });

            Assert.AreEqual(1, listener.deployedScenes.Count);
            Assert.AreEqual(0, listener.setScenes.Count);
            Assert.AreEqual(0, listener.addedScenes.Count);
            Assert.AreEqual(0, listener.removedScenes.Count);
            Assert.AreEqual(1, listener.projectScenes.Count);

            listener.Clear();

            scenesViewController.SetScenes(new List<ISceneData>()
            {
                new SceneData(){id = "1", isDeployed = false},
                new SceneData(){id = "2", isDeployed = false}
            });

            Assert.AreEqual(0, listener.deployedScenes.Count);
            Assert.AreEqual(0, listener.setScenes.Count);
            Assert.AreEqual(1, listener.addedScenes.Count);
            Assert.AreEqual(1, listener.removedScenes.Count);
            Assert.AreEqual(2, listener.projectScenes.Count);
        }
    }

    class Listener : IDeployedSceneListener, IProjectSceneListener
    {
        public List<string> setScenes = new List<string>();
        public List<string> addedScenes = new List<string>();
        public List<string> removedScenes = new List<string>();

        public List<string> deployedScenes = new List<string>();
        public List<string> projectScenes = new List<string>();

        public void Clear()
        {
            setScenes.Clear();
            addedScenes.Clear();
            removedScenes.Clear();
        }

        void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
        {
            foreach (var view in scenes.Values)
            {
                setScenes.Add(view.sceneData.id);
                deployedScenes.Add(view.sceneData.id);
            }
        }

        void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
        {
            addedScenes.Add(scene.sceneData.id);
            deployedScenes.Add(scene.sceneData.id);
        }

        void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
        {
            removedScenes.Add(scene.sceneData.id);
            deployedScenes.Remove(scene.sceneData.id);
        }

        void IProjectSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
        {
            foreach (var view in scenes.Values)
            {
                setScenes.Add(view.sceneData.id);
                projectScenes.Add(view.sceneData.id);
            }
        }

        void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
        {
            addedScenes.Add(scene.sceneData.id);
            projectScenes.Add(scene.sceneData.id);
        }

        void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
        {
            removedScenes.Add(scene.sceneData.id);
            projectScenes.Remove(scene.sceneData.id);
        }
    }
}