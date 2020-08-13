using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSyncController
{
    public static bool transformSyncDirty = false;

    public PhysicsSyncController()
    {
        Physics.autoSimulation = false;
        Physics.autoSyncTransforms = false;
    }

    public void Update()
    {
        if (!transformSyncDirty)
            return;

        transformSyncDirty = false;
        Physics.SyncTransforms();
    }
}