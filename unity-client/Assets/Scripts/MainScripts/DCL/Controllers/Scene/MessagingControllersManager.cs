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

        public void Initialize(IMessageHandler messageHandler)
        {
            throttler = new MessageThrottlingController();
            messagingControllers[GLOBAL_MESSAGING_CONTROLLER] = new MessagingController(messageHandler, GLOBAL_MESSAGING_CONTROLLER);

            if (mainCoroutine == null)
            {
                SceneController.i.OnSortScenes += OnSortScenes;
                DCLCharacterController.OnCharacterMoved += OnCharacterMoved;
                mainCoroutine = SceneController.i.StartCoroutine(ProcessMessages());
            }
        }

        private void OnCharacterMoved(DCLCharacterPosition obj)
        {
            string currentSceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

            if (!string.IsNullOrEmpty(currentSceneId))
                messagingControllers.TryGetValue(currentSceneId, out currentSceneController);
        }

        List<MessagingController> sortedControllers = new List<MessagingController>();
        int sortedControllersCount = 0;
        private void OnSortScenes()
        {
            List<ParcelScene> scenesSortedByDistance = SceneController.i.scenesSortedByDistance;

            int count = scenesSortedByDistance.Count;   // we need to retrieve list count everytime because it
                                                        // may change after a yield return

            string currentSceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);
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

                    if (messagingControllers[controllerId].sleeping)
                        continue;

                    sortedControllers.Add(messagingControllers[controllerId]);
                }
            }

            sortedControllersCount = sortedControllers.Count;
        }



        public void Stop()
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

            messagingControllers.Clear();
        }


        public bool ContainsController(string sceneId)
        {
            return messagingControllers.ContainsKey(sceneId);
        }

        MessagingController globalController = null;
        MessagingController uiSceneController = null;
        MessagingController currentSceneController = null;


        public void AddController(IMessageHandler messageHandler, string sceneId, bool isGlobal = false)
        {
            if (!messagingControllers.ContainsKey(sceneId))
            {
                messagingControllers[sceneId] = new MessagingController(messageHandler, sceneId);
            }

            if (isGlobal)
                globalSceneId = sceneId;

            if (!string.IsNullOrEmpty(globalSceneId))
                messagingControllers.TryGetValue(globalSceneId, out uiSceneController);

            if (!string.IsNullOrEmpty(GLOBAL_MESSAGING_CONTROLLER))
                messagingControllers.TryGetValue(GLOBAL_MESSAGING_CONTROLLER, out globalController);
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

        IEnumerator ProcessMessages()
        {
            float prevTimeBudget;
            IEnumerator yieldReturn;
            float start;

            while (true)
            {
                prevTimeBudget = INIT_MSG_BUS_BUDGET_MAX;
                start = Time.unscaledTime;
                bool processedBus = false;
                // When breaking this second loop, we skip a frame
                while (true)
                {
                    //-------------------------------------------------------------------------------------------
                    // Global scene UI
                    if (uiSceneController != null && !uiSceneController.sleeping)
                    {
                        processedBus = true;
                        if (ProcessBus(uiSceneController.uiBus, ref prevTimeBudget, out yieldReturn))
                            break;
                    }

                    //-------------------------------------------------------------------------------------------
                    // Global Controller INIT
                    if (globalController != null && !globalController.sleeping)
                    {
                        processedBus = true;
                        if (ProcessBus(globalController.initBus, ref prevTimeBudget, out yieldReturn))
                            break;
                    }

                    //-------------------------------------------------------------------------------------------
                    // Global scene INIT
                    if (uiSceneController != null && !uiSceneController.sleeping)
                    {
                        processedBus = true;
                        if (ProcessBus(uiSceneController.initBus, ref prevTimeBudget, out yieldReturn))
                            break;
                    }

                    //-------------------------------------------------------------------------------------------
                    // Current Scene INIT, UI and SYSTEM
                    if (currentSceneController != null && !currentSceneController.sleeping)
                    {
                        processedBus = true;
                        if (ProcessBus(currentSceneController.initBus, ref prevTimeBudget, out yieldReturn))
                            break;

                        if (ProcessBus(currentSceneController.uiBus, ref prevTimeBudget, out yieldReturn))
                            break;

                        if (ProcessBus(currentSceneController.systemBus, ref prevTimeBudget, out yieldReturn))
                            break;
                    }

                    //-------------------------------------------------------------------------------------------
                    // Rest of the scenes INIT
                    bool shouldRestart = false;

                    for (int i = 0; i < sortedControllersCount; i++)
                    {
                        MessagingController msgController = sortedControllers[i];
                        processedBus = true;

                        if (ProcessBus(msgController.initBus, ref prevTimeBudget, out yieldReturn))
                        {
                            shouldRestart = true;
                            break;
                        }
                    }


                    if (shouldRestart)
                        break;

                    //-------------------------------------------------------------------------------------------
                    // Rest of the scenes UI

                    for (int i = 0; i < sortedControllersCount; i++)
                    {
                        MessagingController msgController = sortedControllers[i];
                        processedBus = true;

                        if (ProcessBus(msgController.uiBus, ref prevTimeBudget, out yieldReturn))
                        {
                            shouldRestart = true;
                            break;
                        }
                    }

                    if (shouldRestart)
                        break;

                    //-------------------------------------------------------------------------------------------
                    // Global scene SYSTEM
                    if (uiSceneController != null && !uiSceneController.sleeping)
                    {
                        processedBus = true;
                        if (ProcessBus(uiSceneController.systemBus, ref prevTimeBudget, out yieldReturn))
                            break;
                    }

                    //-------------------------------------------------------------------------------------------
                    // Rest of the scenes SYSTEM

                    for (int i = 0; i < sortedControllersCount; i++)
                    {
                        MessagingController msgController = sortedControllers[i];

                        processedBus = true;

                        if (ProcessBus(msgController.systemBus, ref prevTimeBudget, out yieldReturn))
                        {
                            shouldRestart = true;
                            break;
                        }
                    }

                    if (shouldRestart || Time.realtimeSinceStartup - start >= GLOBAL_MAX_MSG_BUDGET || !processedBus)
                        break;
                }

                yield return null;
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

                bus.owner?.RefreshSleepingState();

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
