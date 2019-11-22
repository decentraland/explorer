using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("CameraTests")]
public class CameraController : MonoBehaviour
{
    public enum CameraState
    {
        FirstPerson,
        ThirdPerson
    }

    [SerializeField] internal Camera mainCamera;
    [SerializeField] internal CameraStateSO currentState;
    [SerializeField] internal FirstPersonCameraConfigSO firstPersonConfig;
    [SerializeField] internal ThirdPersonCameraConfigSO thirdPersonConfig;

    internal BaseVariable<CameraState>.Change cameraStateChangedDelegate;
    internal Dictionary<CameraState, CameraSetup> cameraSetups;
    internal CameraSetup currentSetup;

    private void Awake()
    {
        InitializeStates();
        cameraStateChangedDelegate = (newState, oldState) => { SetState(newState); };

        SetState(currentState);
        currentState.OnChange += cameraStateChangedDelegate;
    }

    private void OnDestroy()
    {
        currentState.OnChange -= cameraStateChangedDelegate;
    }

    private void InitializeStates()
    {
        cameraSetups = new Dictionary<CameraState, CameraSetup>()
        {
            { CameraState.FirstPerson, CameraSetupFactory.CreateCameraSetup(CameraState.FirstPerson, mainCamera, firstPersonConfig) },
            { CameraState.ThirdPerson, CameraSetupFactory.CreateCameraSetup(CameraState.ThirdPerson, mainCamera, thirdPersonConfig) },
        };
    }

    private void SetState(CameraState newState)
    {
        if (cameraSetups.ContainsKey(newState))
        {
            SetState(cameraSetups[newState]);
        }
    }

    private void SetState(CameraSetup newSetup)
    {
        currentSetup?.Deactivate();
        currentSetup = newSetup;
        currentSetup?.Activate();
    }

    //This will likely be moved to the InputController or an equivalent class. It's simple enough to leave it for the PoC 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (currentState == CameraState.FirstPerson)
                currentState.Set(CameraState.ThirdPerson);
            else
                currentState.Set(CameraState.FirstPerson);
        }
        currentSetup?.Update(Time.deltaTime);
    }
}

public static class CameraSetupFactory
{
    public static CameraSetup CreateCameraSetup<T>(CameraController.CameraState cameraState, Camera mainCamera, T config)
    {
        switch (cameraState)
        {
            case CameraController.CameraState.FirstPerson:
                if (config is BaseVariable<FirstPersonCameraConfig> firstPersonConfig)
                    return new FirstPersonCameraSetup(mainCamera, firstPersonConfig);
                break;
            case CameraController.CameraState.ThirdPerson:
                if (config is BaseVariable<ThirdPersonCameraConfig> thirdPersonConfig)
                    return new ThirdPersonCameraSetup(mainCamera, thirdPersonConfig);
                break;
        }
        return null;
    }
}