using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MessagingControllersManager : Singleton<MessagingControllersManager>
    {
        public static bool VERBOSE = false;

        private const float GLOBAL_MAX_MSG_BUDGET = 0.016f;
        private const float GLOBAL_MAX_MSG_BUDGET_WHEN_LOADING = 1f;
        private const float GLOBAL_MIN_MSG_BUDGET_WHEN_LOADING = 1f;
        public const float UI_MSG_BUS_BUDGET_MAX = 0.013f;
        public const float INIT_MSG_BUS_BUDGET_MAX = 0.016f;
        public const float SYSTEM_MSG_BUS_BUDGET_MAX = 0.013f;
        public const float MSG_BUS_BUDGET_MIN = 0.00001f;
        private const float GLTF_BUDGET_MAX = 0.033f;
        private const float GLTF_BUDGET_MIN = 0.008f;

        public const string GLOBAL_MESSAGING_CONTROLLER = "global_messaging_controller";

        public Dictionary<string, MessagingController> messagingControllers = new Dictionary<string, MessagingController>();
        private string globalSceneId = null;

        private Coroutine mainCoroutine;

        public bool hasPendingMessages => pendingMessagesCount > 0;
        public MessageThrottlingController throttler;

        public int pendingMessagesCount;
        public int pendingInitMessagesCount;
        public long processedInitMessagesCount;

        public bool isRunning { get { return mainCoroutine != null; } }

        private List<MessagingController> sortedControllers = new List<MessagingController>();
        private int sortedControllersCount = 0;

        private MessagingController globalController = null;
        private MessagingController uiSceneController = null;
        private MessagingController currentSceneController = null;

        public void Initialize(IMessageHandler messageHandler)
        {
            throttler = new MessageThrottlingController();

            messagingControllers[GLOBAL_MESSAGING_CONTROLLER] = new MessagingController(messageHandler, GLOBAL_MESSAGING_CONTROLLER);

            if (!string.IsNullOrEmpty(GLOBAL_MESSAGING_CONTROLLER))
                messagingControllers.TryGetValue(GLOBAL_MESSAGING_CONTROLLER, out globalController);

            SceneController.i.OnSortScenes += RefreshScenesState;
            DCLCharacterController.OnCharacterMoved += OnCharacterMoved;

            if (mainCoroutine == null)
            {
                mainCoroutine = SceneController.i.StartCoroutine(ProcessMessages());
            }
        }

        private void OnCharacterMoved(DCLCharacterPosition obj)
        {
            string currentSceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

            if (!string.IsNullOrEmpty(currentSceneId))
                messagingControllers.TryGetValue(currentSceneId, out currentSceneController);
        }

        public void RefreshScenesState()
        {
            List<ParcelScene> scenesSortedByDistance = SceneController.i.scenesSortedByDistance;

            int count = scenesSortedByDistance.Count;   // we need to retrieve list count everytime because it
                                                        // may change after a yield return

            string currentSceneId = null;

            if (SceneController.i != null && DCLCharacterController.i != null)
                currentSceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

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

                    if (!messagingControllers[controllerId].enabled)
                        continue;

                    sortedControllers.Add(messagingControllers[controllerId]);
                }
            }

            sortedControllersCount = sortedControllers.Count;
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

            SceneController.i.OnSortScenes -= RefreshScenesState;
            DCLCharacterController.OnCharacterMoved -= OnCharacterMoved;

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
        }

        public void RemoveController(string sceneId)
        {
            if (messagingControllers.ContainsKey(sceneId))
            {
                // In case there is any pending message from a scene being unloaded we decrease the count accordingly
                pendingMessagesCount -= messagingControllers[sceneId].messagingBuses[MessagingBusId.INIT].pendingMessages.Count +
                                        messagingControllers[sceneId].messagingBuses[MessagingBusId.UI].pendingMessages.Count +
                                        messagingControllers[sceneId].messagingBuses[MessagingBusId.SYSTEM].pendingMessages.Count;

                DisposeController(messagingControllers[sceneId]);
                messagingControllers.Remove(sceneId);
            }
        }

        void DisposeController(MessagingController controller)
        {
            controller.Stop();
            controller.Dispose();
        }

        public string Enqueue(ParcelScene scene, MessagingBus.QueuedSceneMessage_Scene queuedMessage)
        {
            string busId = "";

            messagingControllers[queuedMessage.sceneId].Enqueue(scene, queuedMessage, out busId);

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
            }
        }


        public void UpdateThrottling()
        {
            if (pendingInitMessagesCount == 0)
            {
                UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = Mathf.Clamp(throttler.currentTimeBudget, GLTF_BUDGET_MIN, GLTF_BUDGET_MAX) * 1000f;
            }
            else
            {
                UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds = 0;
            }
        }

        /**
         * Calls to ProcessBus to run events from this queue. Returns true if ProcessBus returns true
         */
        private bool ProcessEventsFromBus(MessagingController controller, MessagingBus bus, ref float prevTimeBudget, ref IEnumerator yieldReturn)
        {
            if (controller.enabled)
            {
                if (ProcessBus(bus, ref prevTimeBudget, out yieldReturn))
                    return true;
            }
            return false;
        }

        IEnumerator ProcessMessages()
        {
            float prevTimeBudget;
            IEnumerator yieldReturn = null;
            float start;

            while (true)
            {
                prevTimeBudget = INIT_MSG_BUS_BUDGET_MAX;
                start = Time.unscaledTime;

                while (true)
                {
                    // If in the previous iteration we found a corutine to run, call it
                    if (yieldReturn != null)
                        yield return yieldReturn;

                    // If we spent too much time in this loop, continue processing in the next frame by returning null
                    if (Time.realtimeSinceStartup - start >= GLOBAL_MAX_MSG_BUDGET)
                        yield return null;

                    // High priority buses: UI events, global initialization events, and UI initialization events
                    if (uiSceneController != null && ProcessEventsFromBus(uiSceneController, uiSceneController.uiBus, ref prevTimeBudget, ref yieldReturn))
                        break;
                    if (globalController != null && ProcessEventsFromBus(globalController, globalController.initBus, ref prevTimeBudget, ref yieldReturn))
                        break;
                    if (uiSceneController != null && ProcessEventsFromBus(uiSceneController, uiSceneController.initBus, ref prevTimeBudget, ref yieldReturn))
                        break;

                    // Next in priority: events for the current scene
                    if (currentSceneController != null && ProcessEventsFromBus(currentSceneController, currentSceneController.initBus, ref prevTimeBudget, ref yieldReturn))
                        break;
                    if (currentSceneController != null && ProcessEventsFromBus(currentSceneController, currentSceneController.uiBus, ref prevTimeBudget, ref yieldReturn))
                        break;
                    if (currentSceneController != null && ProcessEventsFromBus(currentSceneController, currentSceneController.systemBus, ref prevTimeBudget, ref yieldReturn))
                        break;

                    // Then: other initialization events from scenes
                    bool shouldRestart = false;
                    for (int i = 0; i < sortedControllersCount; i++)
                    {
                        if (ProcessEventsFromBus(sortedControllers[i], sortedControllers[i].initBus, ref prevTimeBudget, ref yieldReturn)) { 
                            shouldRestart = true;
                            break;
                        }
                    }
                    if (shouldRestart)
                        break;

                    // Low priority: UI events for scenes
                    for (int i = 0; i < sortedControllersCount; i++)
                    {
                        if (ProcessEventsFromBus(sortedControllers[i], sortedControllers[i].uiBus, ref prevTimeBudget, ref yieldReturn))
                        {
                            shouldRestart = true;
                            break;
                        }
                    }
                    if (shouldRestart)
                        break;

                    // Lower priority: UI events for the UI scene
                    if (uiSceneController != null && ProcessEventsFromBus(uiSceneController, uiSceneController.systemBus, ref prevTimeBudget, ref yieldReturn))
                        break;

                    // Lowest priority: system events for all the other scenes
                    // TODO: Shouldn't we ignore these? After all, UI events are only enabled for the current scene
                    for (int i = 0; i < sortedControllersCount; i++)
                    {
                        if (ProcessEventsFromBus(sortedControllers[i], sortedControllers[i].systemBus, ref prevTimeBudget, ref yieldReturn))
                        {
                            shouldRestart = true;
                            break;
                        }
                    }
                    if (shouldRestart)
                        break;

                    // If we have not processed any messages on any bus, we're done here!
                    yield return null;
                }
            }
        }

        bool ProcessBus(MessagingBus bus, ref float prevTimeBudget, out IEnumerator yieldReturn)
        {
            if (bus.isRunning && bus.pendingMessagesCount > 0)
            {
                float startTime = Time.realtimeSinceStartup;

                yieldReturn = null;

                float timeBudget = prevTimeBudget;

                if (RenderingController.i.renderingEnabled)
                    timeBudget = Mathf.Clamp(timeBudget, bus.budgetMin, bus.budgetMax);
                else
                    timeBudget = Mathf.Clamp(timeBudget, GLOBAL_MIN_MSG_BUDGET_WHEN_LOADING, GLOBAL_MAX_MSG_BUDGET_WHEN_LOADING);

                if (VERBOSE && timeBudget == 0)
                {
                    string finalTag = SceneController.i.TryToGetSceneCoordsID(bus.debugTag);
                    Debug.Log($"#{bus.processedMessagesCount} ... bus = {finalTag}, id = {bus.id}... timeBudget is zero!!!");
                }

                bool queueResult = bus.ProcessQueue(timeBudget, out yieldReturn);

                bus.owner?.RefreshEnabledState();

                if (queueResult)
                    return true;

                prevTimeBudget -= Time.realtimeSinceStartup - startTime;

                if (prevTimeBudget <= 0)
                {
                    return true;
                }
            }
            else
            {
                yieldReturn = null;
                return false;
            }

            return false;
        }
    }
}
