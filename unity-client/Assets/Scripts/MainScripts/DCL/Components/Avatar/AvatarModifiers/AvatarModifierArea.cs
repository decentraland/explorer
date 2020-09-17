using System;
using DCL;
using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarModifierArea : BaseComponent
{
    [Serializable]
    public class Model
    {
        // TODO: Change to TriggerArea and handle deserialization with subclasses
        public BoxTriggerArea area;
        public string[] modifiers;

    }

    [HideInInspector]
    public Model model = new Model();

    private event Action<AvatarShape> OnAvatarEnter;
    private event Action<AvatarShape> OnAvatarExit;
    private readonly Dictionary<string, AvatarModifier> modifiers;
    private Collider collider;

    public AvatarModifierArea()
    {
        // Configure all available modifiers
        this.modifiers = new Dictionary<string, AvatarModifier>()
        {
            { "HIDE_AVATARS", new HideAvatarsModifier() },
            { "DISABLE_PASSPORTS", new DisablePassportModifier() }
        };
    }

    public override IEnumerator ApplyChanges(string newJson)
    {

        // Clean up previous listeners/colliders
        OnAvatarEnter = null;
        OnAvatarExit = null;
        Destroy(collider);

        model = SceneController.i.SafeFromJson<Model>(newJson);
        if (model.modifiers != null)
        {
            collider = model.area.AddCollider(gameObject);

            // Add all listeners
            foreach (string modifierKey in model.modifiers)
            {
                AvatarModifier modifier = this.modifiers[modifierKey];
                OnAvatarEnter += modifier.ApplyModifier;
                OnAvatarExit += modifier.RemoveModifier;
            }
        }

        yield break;
    }

    private void OnDestroy()
    {
        Destroy(collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        AvatarShape avatar = other.transform.parent?.GetComponent<AvatarShape>();

        if (avatar == null) return;

        OnAvatarEnter?.Invoke(avatar);
    }

    private void OnTriggerExit(Collider other)
    {
        AvatarShape avatar = other.transform.parent?.GetComponent<AvatarShape>();

        if (avatar == null) return;

        OnAvatarExit?.Invoke(avatar);
    }

}
