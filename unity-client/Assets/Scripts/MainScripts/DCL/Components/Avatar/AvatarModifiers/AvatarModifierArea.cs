using System;
using DCL;
using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using DCL.Models;

public class AvatarModifierArea : BaseComponent
{
    [Serializable]
    public class Model : BaseModel
    {
        // TODO: Change to TriggerArea and handle deserialization with subclasses
        public BoxTriggerArea area;
        public string[] modifiers;

        public override bool Equals(object obj)
        {
            return obj is Model model &&
                   EqualityComparer<BoxTriggerArea>.Default.Equals(area, model.area) &&
                   EqualityComparer<string[]>.Default.Equals(modifiers, model.modifiers);
        }

        public override BaseModel GetDataFromJSON(string json)
        {
            return Utils.SafeFromJson<Model>(json);
        }

        public override int GetHashCode()
        {
            int hashCode = -64555011;
            hashCode = hashCode * -1521134295 + EqualityComparer<BoxTriggerArea>.Default.GetHashCode(area);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(modifiers);
            return hashCode;
        }
    }

    [HideInInspector]
    public Model model = new Model();

    private HashSet<GameObject> avatarsInArea = new HashSet<GameObject>();
    private event Action<GameObject> OnAvatarEnter;
    private event Action<GameObject> OnAvatarExit;
    internal readonly Dictionary<string, AvatarModifier> modifiers;

    public AvatarModifierArea()
    {
        // Configure all available modifiers
        this.modifiers = new Dictionary<string, AvatarModifier>()
        {
            { "HIDE_AVATARS", new HideAvatarsModifier() },
            { "DISABLE_PASSPORTS", new DisablePassportModifier() }
        };
    }

    public override IEnumerator ApplyChanges(BaseModel newModel)
    {

        // Clean up
        RemoveAllModifiers();
        OnAvatarEnter = null;
        OnAvatarExit = null;

        ApplyCurrentModel();

        yield break;
    }

    private void OnDestroy()
    {
        var toRemove = new HashSet<GameObject>();
        if(avatarsInArea != null)
            toRemove.UnionWith(avatarsInArea);

        var currentInArea = DetectAllAvatarsInArea();
        if(currentInArea != null)
            toRemove.UnionWith(currentInArea);

        RemoveAllModifiers(toRemove);
    }

    private void Update()
    {
        if (model?.area == null)
        {
            return;
        }

        // Find avatars currently on the area
        HashSet<GameObject> newAvatarsInArea = DetectAllAvatarsInArea();
        if (AreSetEquals(avatarsInArea, newAvatarsInArea))
            return;

        if(avatarsInArea == null)
            avatarsInArea = new HashSet<GameObject>();

        if(newAvatarsInArea == null)
            newAvatarsInArea = new HashSet<GameObject>();

        // Call event for avatars that just entered the area
        foreach (GameObject avatarThatEntered in newAvatarsInArea.Except(avatarsInArea))
        {
            OnAvatarEnter?.Invoke(avatarThatEntered);
        }

        // Call events for avatars that just exited the area
        foreach (GameObject avatarThatExited in avatarsInArea.Except(newAvatarsInArea))
        {
            OnAvatarExit?.Invoke(avatarThatExited);
        }

        avatarsInArea = newAvatarsInArea;
    }

    private bool AreSetEquals(HashSet<GameObject> set1, HashSet<GameObject> set2)
    {
        if (set1 == null && set2 == null)
            return true;

        if (set1 == null || set2 == null)
            return false;

        return set1.SetEquals(set2);
    }

    private HashSet<GameObject> DetectAllAvatarsInArea()
    {
        if (entity?.gameObject == null)
        {
            return null;
        }

        Vector3 center = entity.gameObject.transform.position;
        Quaternion rotation = entity.gameObject.transform.rotation;
        return model.area.DetectAvatars(center, rotation);
    }

    private void RemoveAllModifiers()
    {
        RemoveAllModifiers(DetectAllAvatarsInArea());
    }

    private void RemoveAllModifiers(HashSet<GameObject> avatars)
    {
        if (model?.area == null)
        {
            return;
        }

        if (avatars != null)
        {
            foreach (GameObject avatar in avatars)
            {
                OnAvatarExit?.Invoke(avatar);
            }
        }
    }

    private void ApplyCurrentModel()
    {
        if (model.modifiers != null)
        {
            // Add all listeners
            foreach (string modifierKey in model.modifiers)
            {
                if (!modifiers.TryGetValue(modifierKey, out AvatarModifier modifier))
                    continue;

                OnAvatarEnter += modifier.ApplyModifier;
                OnAvatarExit += modifier.RemoveModifier;
            }
        }
    }

    public override int GetClassId()
    {
        return (int) CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA;
    }
}
