using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class TriggerArea
{
    public abstract GameObject[] DetectAvatars(Vector3 center, Quaternion rotation);

}

[System.Serializable]
public class BoxTriggerArea : TriggerArea
{
    public Vector3 box;

    public override GameObject[] DetectAvatars(Vector3 center, Quaternion rotation)
    {
        Collider[] colliders = Physics.OverlapBox(center, box / 2, rotation, LayerMask.GetMask("AvatarTriggerDetection"), QueryTriggerInteraction.Collide);
        return colliders.Select(collider => collider.transform.parent.gameObject)
            .ToArray();
    }
}
