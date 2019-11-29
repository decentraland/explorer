using System;
using UnityEngine;

public enum DCLAction_OneTime
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    Jump = 1,
    CameraChange = 100,
}

public enum DCLAction_InTime
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    Sprint = 1,
    FreeCameraMode = 101,
}

public enum DCLAction_Measurable
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    CharacterXAxis = 1,
    CharacterYAxis = 2,
    CameraXAxis = 3,
    CameraYAxis = 4,
}

public class InputController : MonoBehaviour
{
    public InputAction_OneTime[] oneTimeActions;
    public InputAction_InTime[] inTimeActions;
    public InputAction_Measurable[] measurableActions;

    private void Update()
    {
        Update_OneTime();
        Update_InTime();
        Update_Measurable();
    }

    public void Update_OneTime()
    {
        for (var i = 0; i < oneTimeActions.Length; i++)
        {
            var action = oneTimeActions[i];
            switch (action.GetDCLAction())
            {
                case DCLAction_OneTime.Jump:
                    InputProcessor.FromKey( action, KeyCode.Space, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_OneTime.CameraChange:
                    InputProcessor.FromKey( action, KeyCode.V, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Update_InTime()
    {
        for (var i = 0; i < inTimeActions.Length; i++)
        {
            var action = inTimeActions[i];
            switch (action.GetDCLAction())
            {
                case DCLAction_InTime.Sprint:
                    InputProcessor.FromKey( action, KeyCode.LeftShift, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_InTime.FreeCameraMode:
                    InputProcessor.FromKey( action, KeyCode.T, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Update_Measurable()
    {
        for (var i = 0; i < measurableActions.Length; i++)
        {
            var action = measurableActions[i];
            switch (action.GetDCLAction())
            {
                case DCLAction_Measurable.CharacterXAxis:
                    InputProcessor.FromAxis( action, "Horizontal", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CharacterYAxis:
                    InputProcessor.FromAxis( action, "Vertical", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CameraXAxis:
                    InputProcessor.FromAxis( action, "Mouse X", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CameraYAxis:
                    InputProcessor.FromAxis( action, "Mouse Y", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

public static class InputProcessor
{
    [Flags]
    public enum Modifier
    {
        //Set the values as bit masks
        None = 0b0000000,
        NeedsPointerLocked = 0b0000001,
    }

    public static bool PassModifiers(Modifier modifiers)
    {
        bool result = true;

        if (IsModifierSet(modifiers, Modifier.NeedsPointerLocked) && Cursor.lockState != CursorLockMode.Locked)
            return false;

        return true;
    }

    public static void FromKey(InputAction_OneTime action, KeyCode key, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers)) return;

        if (Input.GetKeyDown(key)) action.RaiseOnTriggered();
    }

    public static void FromMouseButton(InputAction_OneTime action, int mouseButtonIdx, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers)) return;

        if (Input.GetMouseButton(mouseButtonIdx)) action.RaiseOnTriggered();
    }

    public static void FromKey(InputAction_InTime action, KeyCode key, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers)) return;

        if (Input.GetKeyDown(key)) action.RaiseOnStarted();
        if (Input.GetKeyUp(key)) action.RaiseOnFinished();
    }

    public static void FromMouse(InputAction_InTime action, int mouseButtonIdx, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers)) return;

        if (Input.GetMouseButtonDown(mouseButtonIdx)) action.RaiseOnStarted();
        if (Input.GetMouseButtonUp(mouseButtonIdx)) action.RaiseOnFinished();
    }

    public static void FromAxis(InputAction_Measurable action, string axisName, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers))
        {
            action.RaiseOnValueChanged(0);
            return;
        }

        action.RaiseOnValueChanged(Input.GetAxis(axisName));
    }

    public static bool IsModifierSet(Modifier modifiers, Modifier value)
    {
        int flagsValue = (int)modifiers;
        int flagValue = (int)value;

        return (flagsValue & flagValue) != 0;
    }
}