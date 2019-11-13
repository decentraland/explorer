﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    [RequireComponent(typeof(StatsPanel))]
    public class MiscBenchmarkController : MonoBehaviour, IBenchmarkController
    {
        public enum Columns
        {
            NONE,
            VALUE,
        }

        public enum Rows
        {
            NONE,
            SHARED_OBJECTS_COUNT,
            COMPONENT_OBJECTS_COUNT,
            ENTITY_OBJECTS_COUNT,
            BREAK_0,
            MATERIAL_COUNT,
            MESHES_COUNT,
            BREAK_1,
            GLTF_BEING_LOADED,
            MESSAGES_PER_SECOND_REAL,
            CPU_SCHEDULER,
            MESSAGE_BUSES,
        }

        const string BREAK_0_TEXT = "";
        const string BREAK_1_TEXT = "";

        const string SHARED_OBJECTS_COUNT_TEXT = "Shared Objects Count";
        const string COMPONENT_OBJECTS_COUNT_TEXT = "Components Count";
        const string ENTITY_OBJECTS_COUNT_TEXT = "Entity Count";
        const string MATERIAL_COUNT_TEXT = "Material Count";
        const string MESHES_COUNT_TEXT = "Meshes Count";
        const string GLTF_BEING_LOADED_TEXT = "GLTFs being loaded";
        const string MESSAGES_PER_SECOND_REAL_TEXT = "Messages x sec";
        const string CPU_SCHEDULER_TEXT = "CPU Scheduler";
        const string MESSAGES_BUSES_TEXT = "Message buses";

        StatsPanel statsPanel;
        List<Columns> columnsList;
        List<Rows> rowsList;

        int lastPendingMessages;
        int sampleCount = 0;
        float mps = 0;

        public void Init()
        {
            this.statsPanel = GetComponent<StatsPanel>();

            columnsList = Enum.GetValues(typeof(Columns)).Cast<Columns>().ToList();
            rowsList = Enum.GetValues(typeof(Rows)).Cast<Rows>().ToList();

            statsPanel.PopulateTable(columnsList.Count, rowsList.Count, 240, 240);

            //NOTE(Brian): Top-left cell, unused.
            statsPanel.SetCellText(0, 0, "");

            //NOTE(Brian): Row stuff (left vertical header)
            statsPanel.SetCellText(0, (int)Rows.SHARED_OBJECTS_COUNT, SHARED_OBJECTS_COUNT_TEXT);
            statsPanel.SetCellText(0, (int)Rows.COMPONENT_OBJECTS_COUNT, COMPONENT_OBJECTS_COUNT_TEXT);
            statsPanel.SetCellText(0, (int)Rows.ENTITY_OBJECTS_COUNT, ENTITY_OBJECTS_COUNT_TEXT);
            statsPanel.SetCellText(0, (int)Rows.BREAK_0, BREAK_0_TEXT);
            statsPanel.SetCellText(0, (int)Rows.MATERIAL_COUNT, MATERIAL_COUNT_TEXT);
            statsPanel.SetCellText(0, (int)Rows.MESHES_COUNT, MESHES_COUNT_TEXT);
            statsPanel.SetCellText(0, (int)Rows.BREAK_1, BREAK_1_TEXT);
            statsPanel.SetCellText(0, (int)Rows.GLTF_BEING_LOADED, GLTF_BEING_LOADED_TEXT);
            statsPanel.SetCellText(0, (int)Rows.MESSAGES_PER_SECOND_REAL, MESSAGES_PER_SECOND_REAL_TEXT);
            statsPanel.SetCellText(0, (int)Rows.CPU_SCHEDULER, CPU_SCHEDULER_TEXT);
            statsPanel.SetCellText(0, (int)Rows.MESSAGE_BUSES, MESSAGES_BUSES_TEXT);
        }

        public void StartProfiling()
        {
            if (enabled)
            {
                return;
            }

            if (statsPanel == null)
            {
                Init();
            }

            SceneController.i.StartCoroutine(RefreshProfilingData());
            enabled = true;
        }

        public void StopProfiling()
        {
            if (!enabled)
            {
                return;
            }

            SceneController.i.StopCoroutine(RefreshProfilingData());
            enabled = false;
        }

        static float budgetMax = 0;
        bool enableBudgetMax = false;

        void Update()
        {
            MessageThrottlingController globalThrottler = MessagingControllersManager.i.throttler;

            if (globalThrottler != null && globalThrottler.currentTimeBudget < 0.001f)
                enableBudgetMax = true;

            int messagesProcessedLastFrame = lastPendingMessages - MessagingControllersManager.i.pendingMessagesCount;

            if (messagesProcessedLastFrame > 0)
            {
                sampleCount++;
                mps += 1 / (Time.deltaTime / messagesProcessedLastFrame);
                statsPanel.SetCellText(1, (int)Rows.MESSAGES_PER_SECOND_REAL, (mps / sampleCount).ToString());
            }

            lastPendingMessages = MessagingControllersManager.i.pendingMessagesCount;
        }


        private IEnumerator RefreshProfilingData()
        {
            while (true)
            {
                int sharedCount = 0;
                int sharedAttachCount = 0;
                int componentCount = 0;
                int entityCount = 0;
                int materialCount = 0;
                int meshesCount = 0;

                foreach (var v in SceneController.i.loadedScenes)
                {
                    if (v.Value.metricsController != null)
                    {
                        meshesCount += v.Value.metricsController.GetModel().meshes;
                        materialCount += v.Value.metricsController.GetModel().materials;
                    }

                    sharedCount += v.Value.disposableComponents.Count;

                    foreach (var e in v.Value.disposableComponents)
                    {
                        sharedAttachCount += e.Value.attachedEntities.Count;
                    }

                    entityCount += v.Value.entities.Count;

                    foreach (var e in v.Value.entities)
                    {
                        componentCount += e.Value.components.Count;
                    }
                }

                statsPanel.SetCellText(1, (int)Rows.SHARED_OBJECTS_COUNT, sharedCount.ToString());
                statsPanel.SetCellText(1, (int)Rows.COMPONENT_OBJECTS_COUNT, componentCount.ToString());
                statsPanel.SetCellText(1, (int)Rows.ENTITY_OBJECTS_COUNT, entityCount.ToString());
                statsPanel.SetCellText(1, (int)Rows.MATERIAL_COUNT, materialCount.ToString());
                statsPanel.SetCellText(1, (int)Rows.MESHES_COUNT, meshesCount.ToString());
                statsPanel.SetCellText(1, (int)Rows.GLTF_BEING_LOADED, UnityGLTF.GLTFComponent.downloadingCount.ToString() + " / " + UnityGLTF.GLTFComponent.queueCount.ToString());

                float rate = 0;
                float budget = 0;

                MessageThrottlingController cpus = MessagingControllersManager.i.throttler;

                if (cpus != null)
                {
                    rate = cpus.messagesConsumptionRate;
                    budget = cpus.currentTimeBudget * 1000f;

                    if (enableBudgetMax)
                        budgetMax = Mathf.Max(cpus.currentTimeBudget, budgetMax);
                }

                statsPanel.SetCellText(1, (int)Rows.CPU_SCHEDULER, $"msgs rate: {rate}\nbudget: {budget}ms\nbudget peak: {budgetMax * 1000f}ms");

                string busesLog = "";
                Dictionary<string, int> pendingMessagesCount = new Dictionary<string, int>();
                Dictionary<string, int> messagesReplaced = new Dictionary<string, int>();

                using (var controllersIter = MessagingControllersManager.i.messagingControllers.GetEnumerator())
                {
                    while (controllersIter.MoveNext())
                    {
                        using (var iterator = controllersIter.Current.Value.messagingBuses.GetEnumerator())
                        {
                            while (iterator.MoveNext())
                            {
                                //access to pair using iterator.Current
                                string key = iterator.Current.Key;
                                MessagingBus bus = controllersIter.Current.Value.messagingBuses[key];

                                if (!pendingMessagesCount.ContainsKey(key))
                                    pendingMessagesCount[key] = 0;

                                if (!messagesReplaced.ContainsKey(key))
                                    messagesReplaced[key] = 0;

                                pendingMessagesCount[key] += bus.pendingMessagesCount;
                                messagesReplaced[key] += bus.unreliableMessagesReplaced;
                            }
                        }
                    }
                }

                busesLog += $"{MessagingBusId.UI} bus: {pendingMessagesCount[MessagingBusId.UI]} replaced: {messagesReplaced[MessagingBusId.UI]}\n";
                busesLog += $"{MessagingBusId.INIT} bus: {pendingMessagesCount[MessagingBusId.INIT]} replaced: {messagesReplaced[MessagingBusId.INIT]}\n";
                busesLog += $"{MessagingBusId.SYSTEM} bus: {pendingMessagesCount[MessagingBusId.SYSTEM]} replaced: {messagesReplaced[MessagingBusId.SYSTEM]}\n";

                statsPanel.SetCellText(1, (int)Rows.MESSAGE_BUSES, busesLog);

                yield return WaitForSecondsCache.Get(0.2f);
            }

        }
    }
}
