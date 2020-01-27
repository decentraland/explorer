using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MessagingControllersManager : Singleton<MessagingControllersManager>
    {
        public static bool VERBOSE = false;

        private const float MAX_GLOBAL_MSG_BUDGET = 0.006f;
        private const float MAX_GLOBAL_MSG_BUDGET_WHEN_INIT = 0.01f;

        private const float GLTF_BUDGET_MAX = 0.008f;

        public const string GLOBAL_MESSAGING_CONTROLLER = "global_messaging_controller";

        public Dictionary<string, MessagingController> messagingControllers = new Dictionary<string, MessagingController>();
        private string globalSceneId = null;

        private Coroutine mainCoroutine;

        public bool hasPendingMessages => pendingMessagesCount > 0;

        public int pendingMessagesCount;
        public int pendingInitMessagesCount;
        public long processedInitMessagesCount;

        public bool isRunning { get { return mainCoroutine != null; } }

        private readonly List<MessagingController> sortedControllers = new List<MessagingController>();
        private readonly List<MessagingBus> busesToProcess = new List<MessagingBus>();
        private int busesToProcessCount = 0;
        private int sortedControllersCount = 0;

        private MessagingController globalController = null;
        private MessagingController uiSceneController = null;
        private MessagingController currentSceneController = null;

        public void Initialize(IMessageHandler messageHandler)
        {
            messagingControllers[GLOBAL_MESSAGING_CONTROLLER] = new MessagingController(messageHandler, GLOBAL_MESSAGING_CONTROLLER);

            if (!string.IsNullOrEmpty(GLOBAL_MESSAGING_CONTROLLER))
                messagingControllers.TryGetValue(GLOBAL_MESSAGING_CONTROLLER, out globalController);

            SceneController.i.OnSortScenes += MarkBusesDirty;

            if (mainCoroutine == null)
            {
                mainCoroutine = SceneController.i.StartCoroutine(ProcessMessages());
            }

            populateBusesDirty = true;
        }

        bool populateBusesDirty = true;
        public void MarkBusesDirty()
        {
            populateBusesDirty = true;
        }

        public void PopulateBusesToBeProcessed()
        {
            string currentSceneId = SceneController.i.CurrentSceneId;
            List<ParcelScene> scenesSortedByDistance = SceneController.i.scenesSortedByDistance;

            int count = scenesSortedByDistance.Count;   // we need to retrieve list count everytime because it
                                                        // may change after a yield return

            sortedControllers.Clear();

            if (!string.IsNullOrEmpty(currentSceneId) && messagingControllers.ContainsKey(currentSceneId))
                currentSceneController = messagingControllers[currentSceneId];

            for (int i = 0; i < count; i++)
            {
                string controllerId = scenesSortedByDistance[i].sceneData.id;

                if (controllerId != currentSceneId)
                {
                    if (!messagingControllers.ContainsKey(controllerId))
                        continue;

                    sortedControllers.Add(messagingControllers[controllerId]);
                }
            }

            sortedControllersCount = sortedControllers.Count;

            bool uiSceneControllerActive = uiSceneController != null;
            bool globalControllerActive = globalController != null;
            bool currentSceneControllerActive = currentSceneController != null;

            busesToProcess.Clear();
            //-------------------------------------------------------------------------------------------
            // Global scene UI
            if (uiSceneControllerActive)
            {
                busesToProcess.Add(uiSceneController.uiBus);
                busesToProcess.Add(uiSceneController.initBus);
                busesToProcess.Add(uiSceneController.systemBus);
            }

            if (globalControllerActive)
            {
                busesToProcess.Add(globalController.initBus);
            }

            if (currentSceneControllerActive)
            {
                busesToProcess.Add(currentSceneController.initBus);
                busesToProcess.Add(currentSceneController.uiBus);
                busesToProcess.Add(currentSceneController.systemBus);
            }

            for (int i = 0; i < sortedControllersCount; ++i)
            {
                MessagingController msgController = sortedControllers[i];
                busesToProcess.Add(msgController.uiBus);
                busesToProcess.Add(msgController.systemBus);
            }

            for (int i = 0; i < sortedControllersCount; ++i)
            {
                MessagingController msgController = sortedControllers[i];
                busesToProcess.Add(msgController.initBus);
            }

            busesToProcessCount = busesToProcess.Count;

            populateBusesDirty = false;
        }

        public void Cleanup()
        {
            if (mainCoroutine != null)
            {
                SceneController.i.StopCoroutine(mainCoroutine);
                mainCoroutine = null;
            }

            using (var controllersIterator = messagingControllers.GetEnumerator())
            {
                while (controllersIterator.MoveNext())
                {
                    controllersIterator.Current.Value.Stop();
                    DisposeController(controllersIterator.Current.Value);
                }
            }

            SceneController.i.OnSortScenes -= PopulateBusesToBeProcessed;

            messagingControllers.Clear();
        }


        public bool ContainsController(string sceneId)
        {
            return messagingControllers.ContainsKey(sceneId);
        }

        public void AddController(IMessageHandler messageHandler, string sceneId, bool isGlobal = false)
        {
            if (!messagingControllers.ContainsKey(sceneId))
                messagingControllers[sceneId] = new MessagingController(messageHandler, sceneId);

            if (isGlobal)
                globalSceneId = sceneId;

            if (!string.IsNullOrEmpty(globalSceneId))
                messagingControllers.TryGetValue(globalSceneId, out uiSceneController);


            populateBusesDirty = true;
        }

        public void RemoveController(string sceneId)
        {
            if (messagingControllers.ContainsKey(sceneId))
            {
                // In case there is any pending message from a scene being unloaded we decrease the count accordingly
                pendingMessagesCount -= messagingControllers[sceneId].messagingBuses[MessagingBusId.INIT].pendingMessagesCount +
                                        messagingControllers[sceneId].messagingBuses[MessagingBusId.UI].pendingMessagesCount +
                                        messagingControllers[sceneId].messagingBuses[MessagingBusId.SYSTEM].pendingMessagesCount;

                DisposeController(messagingControllers[sceneId]);
                messagingControllers.Remove(sceneId);

                populateBusesDirty = true;
            }
        }

        void DisposeController(MessagingController controller)
        {
            controller.Stop();
            controller.Dispose();
        }

        public string Enqueue(ParcelScene scene, MessagingBus.QueuedSceneMessage_Scene queuedMessage)
        {
            messagingControllers[queuedMessage.sceneId].Enqueue(scene, queuedMessage, out string busId);
            return busId;
        }

        public void ForceEnqueueToGlobal(string busId, MessagingBus.QueuedSceneMessage queuedMessage)
        {
            messagingControllers[GLOBAL_MESSAGING_CONTROLLER].ForceEnqueue(busId, queuedMessage);
        }

        public void SetSceneReady(string sceneId)
        {
            // Start processing SYSTEM queue
            if (messagingControllers.ContainsKey(sceneId))
            {
                // Start processing SYSTEM queue
                MessagingController sceneMessagingController = messagingControllers[sceneId];
                sceneMessagingController.StartBus(MessagingBusId.SYSTEM);
                sceneMessagingController.StartBus(MessagingBusId.UI);
                sceneMessagingController.StopBus(MessagingBusId.INIT);

                populateBusesDirty = true;
            }
        }

        float timeBudgetCounter = MAX_GLOBAL_MSG_BUDGET;

        IEnumerator ProcessMessages()
        {
            while (true)
            {
                if (populateBusesDirty)
                {
                    PopulateBusesToBeProcessed();
                    populateBusesDirty = false;
                }

                timeBudgetCounter = RenderingController.i.renderingEnabled ? MAX_GLOBAL_MSG_BUDGET : MAX_GLOBAL_MSG_BUDGET_WHEN_INIT;

                for (int i = 0; i < busesToProcessCount; ++i)
                {
                    MessagingBus bus = busesToProcess[i];

                    if (ProcessBus(bus))
                        break;
                }

                // Never process GLTF info if we have pending messages
                if (pendingInitMessagesCount > 0)
                {
                    UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = 0;
                }
                else
                {
                    UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = Mathf.Clamp(timeBudgetCounter, 0, GLTF_BUDGET_MAX) * 1000f;
                }
                yield return null;
            }
        }

        /**
         * @return true if we run out of time while processing
         */
        bool ProcessBus(MessagingBus bus)
        {
            if (bus.pendingMessagesCount <= 0)
                return false;

            float startTime = Time.realtimeSinceStartup;

            float timeBudget = timeBudgetCounter;

            //TODO(Brian): We should use the returning yieldReturn IEnumerator and MoveNext() it manually each frame to
            //             account the coroutine processing into the budget. Until we do that we just skip it.
            bus.ProcessQueue(timeBudget, out _);

            timeBudgetCounter -= Time.realtimeSinceStartup - startTime;

            if (timeBudgetCounter <= 0)
                return true;

            return false;
        }
    }
}
