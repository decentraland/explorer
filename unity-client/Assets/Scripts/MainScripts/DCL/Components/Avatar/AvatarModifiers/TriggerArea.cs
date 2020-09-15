using UnityEngine;

[System.Serializable]
public abstract class TriggerArea
{
    public abstract void AddCollider(GameObject avatarModifierArea);
    public abstract Collider GetCollider(GameObject avatarModifierArea);

}

[System.Serializable]
public class BoxTriggerArea : TriggerArea
{
    public Vector3 box;

    public override void AddCollider(GameObject avatarModifierArea)
    {
        BoxCollider boxCollider = avatarModifierArea.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.size = box;
    }

    public override Collider GetCollider(GameObject avatarModifierArea)
    {
        return avatarModifierArea.GetComponent<BoxCollider>();
    }
}
