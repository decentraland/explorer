using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class TriggerArea
{
    public abstract HashSet<GameObject> DetectAvatars(Vector3 center, Quaternion rotation);

}

[System.Serializable]
public class BoxTriggerArea : TriggerArea
{
    public Vector3 box;

    public override HashSet<GameObject> DetectAvatars(Vector3 center, Quaternion rotation)
    {
        Collider[] colliders = Physics.OverlapBox(center, box / 2, rotation, LayerMask.GetMask("AvatarTriggerDetection"), QueryTriggerInteraction.Collide);
        HashSet<GameObject> result = new HashSet<GameObject>();
        foreach (Collider collider in colliders)
        {
            result.Add(collider.transform.parent.gameObject);
        }
        return result;
    }
}
