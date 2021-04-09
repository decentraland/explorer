using DCL;
using DCL.Configuration;
using DCL.Helpers;
using UnityEngine;
using System.Collections;
using Cinemachine;
using UnityEngine.SceneManagement;

public class DCLCharacterController : MonoBehaviour
{
    public static DCLCharacterController i { get; private set; }

    [Header("Movement")]
    public float minimumYPosition = 1f;

    public float groundCheckExtraDistance = 0.25f;
    public float gravity = -55f;
    public float jumpForce = 12f;
    public float movementSpeed = 8f;
    public float runningSpeedMultiplier = 2f;

    [Tooltip("The maximum movement distance allowed on moving platforms before releasing the character")]
    public float movingPlatformAllowedPosDelta = 1f;

    public DCLCharacterPosition characterPosition;

    [Header("Collisions")]
    public LayerMask groundLayers;

    [System.NonSerialized]
    public bool initialPositionAlreadySet = false;

    [System.NonSerialized]
    public bool characterAlwaysEnabled = true;

    [System.NonSerialized]
    public CharacterController characterController;

    FreeMovementController freeMovementController;

    new Collider collider;

    float deltaTime = 0.032f;
    float deltaTimeCap = 0.032f; // 32 milliseconds = 30FPS, 16 millisecodns = 60FPS
    float lastUngroundedTime = 0f;
    float lastJumpButtonPressedTime = 0f;
    float lastMovementReportTime;
    float originalGravity;
    Vector3 lastLocalGroundPosition;
    Vector3 velocity = Vector3.zero;

    bool isSprinting = false;
    bool isJumping = false;
    public bool isGrounded { get; private set; }
    public bool isOnMovingPlatform { get; private set; }

    bool supportsMovingPlatforms = true;
    internal Transform groundTransform;
    Vector3 lastPosition;
    Vector3 groundLastPosition;
    Quaternion groundLastRotation;
    bool jumpButtonPressed = false;

    [Header("InputActions")]
    public InputAction_Hold jumpAction;

    public InputAction_Hold sprintAction;

    public Vector3 moveVelocity;

    private InputAction_Hold.Started jumpStartedDelegate;
    private InputAction_Hold.Finished jumpFinishedDelegate;
    private InputAction_Hold.Started sprintStartedDelegate;
    private InputAction_Hold.Finished sprintFinishedDelegate;

    private Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;

    public static System.Action<DCLCharacterPosition> OnCharacterMoved;
    public static System.Action<DCLCharacterPosition> OnPositionSet;
    public event System.Action<float> OnUpdateFinish;


    // Will allow the game objects to be set, and create the DecentralandEntity manually during the Awake
    public DCL.Models.IDCLEntity avatarReference { get; private set; }
    public DCL.Models.IDCLEntity firstPersonCameraReference { get; private set; }

    [SerializeField]
    private GameObject avatarGameObject;

    [SerializeField]
    private GameObject firstPersonCameraGameObject;

    [SerializeField]
    private InputAction_Measurable characterYAxis;

    [SerializeField]
    private InputAction_Measurable characterXAxis;

    private Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
    private Vector3Variable cameraRight => CommonScriptableObjects.cameraRight;

    [System.NonSerialized]
    public float movingPlatformSpeed;

    public event System.Action OnJump;
    public event System.Action OnHitGround;
    public event System.Action<float> OnMoved;

    void Awake()
    {
        if (i != null)
        {
            Destroy(gameObject);
            return;
        }

        i = this;
        originalGravity = gravity;

        SubscribeToInput();
        CommonScriptableObjects.playerUnityPosition.Set(Vector3.zero);
        CommonScriptableObjects.playerWorldPosition.Set(Vector3.zero);
        CommonScriptableObjects.playerCoords.Set(Vector2Int.zero);
        CommonScriptableObjects.playerUnityEulerAngles.Set(Vector3.zero);

        characterPosition = new DCLCharacterPosition();
        characterController = GetComponent<CharacterController>();
        freeMovementController = GetComponent<FreeMovementController>();
        collider = GetComponent<Collider>();

        CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
        Environment.i.platform.debugController.OnDebugModeSet += () => supportsMovingPlatforms = true;

        lastPosition = transform.position;
        transform.parent = null;

        CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
        OnRenderingStateChanged(CommonScriptableObjects.rendererState.Get(), false);

        if (avatarGameObject == null || firstPersonCameraGameObject == null)
        {
            throw new System.Exception("Both the avatar and first person camera game objects must be set.");
        }

        avatarReference = new DCL.Models.DecentralandEntity {gameObject = avatarGameObject};
        firstPersonCameraReference = new DCL.Models.DecentralandEntity {gameObject = firstPersonCameraGameObject};
    }

    private void SubscribeToInput()
    {
        jumpStartedDelegate = (action) =>
        {
            lastJumpButtonPressedTime = Time.time;
            jumpButtonPressed = true;
        };
        jumpFinishedDelegate = (action) => jumpButtonPressed = false;
        jumpAction.OnStarted += jumpStartedDelegate;
        jumpAction.OnFinished += jumpFinishedDelegate;

        sprintStartedDelegate = (action) => isSprinting = true;
        sprintFinishedDelegate = (action) => isSprinting = false;
        sprintAction.OnStarted += sprintStartedDelegate;
        sprintAction.OnFinished += sprintFinishedDelegate;
    }

    void OnDestroy()
    {
        CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
        jumpAction.OnStarted -= jumpStartedDelegate;
        jumpAction.OnFinished -= jumpFinishedDelegate;
        sprintAction.OnStarted -= sprintStartedDelegate;
        sprintAction.OnFinished -= sprintFinishedDelegate;
        CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
    }

    void OnWorldReposition(Vector3 current, Vector3 previous)
    {
        Vector3 oldPos = this.transform.position;
        this.transform.position = characterPosition.unityPosition; //CommonScriptableObjects.playerUnityPosition;

        if (CinemachineCore.Instance.BrainCount > 0)
        {
            CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera?.OnTargetObjectWarped(transform, transform.position - oldPos);
        }
    }

    public void SetPosition(Vector3 newPosition)
    {
        // failsafe in case something teleports the player below ground collisions
        if (newPosition.y < minimumYPosition)
        {
            newPosition.y = minimumYPosition + 2f;
        }

        lastPosition = characterPosition.worldPosition;
        characterPosition.worldPosition = newPosition;
        transform.position = characterPosition.unityPosition;
        Environment.i.platform.physicsSyncController.MarkDirty();

        CommonScriptableObjects.playerUnityPosition.Set(characterPosition.unityPosition);
        CommonScriptableObjects.playerWorldPosition.Set(characterPosition.worldPosition);
        CommonScriptableObjects.playerCoords.Set(Utils.WorldToGridPosition(characterPosition.worldPosition));

        if (Moved(lastPosition))
        {
            if (Moved(lastPosition, useThreshold: true))
                ReportMovement();

            OnCharacterMoved?.Invoke(characterPosition);

            float distance = Vector3.Distance(characterPosition.worldPosition, lastPosition);
            if (distance > 0f && isGrounded)
                OnMoved?.Invoke(distance / Time.deltaTime);
        }

        lastPosition = transform.position;
    }


    public void Teleport(string teleportPayload)
    {
        ResetGround();

        var payload = Utils.FromJsonWithNulls<Vector3>(teleportPayload);

        var newPosition = new Vector3(payload.x, payload.y, payload.z);
        SetPosition(newPosition);

        if (OnPositionSet != null)
        {
            OnPositionSet.Invoke(characterPosition);
        }

        if (!initialPositionAlreadySet)
        {
            initialPositionAlreadySet = true;
        }
    }

    [System.Obsolete("SetPosition is deprecated, please use Teleport instead.", true)]
    public void SetPosition(string positionVector)
    {
        Teleport(positionVector);
    }

    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    bool Moved(Vector3 previousPosition, bool useThreshold = false)
    {
        if (useThreshold)
            return Vector3.Distance(characterPosition.worldPosition, previousPosition) > 0.001f;
        else
            return characterPosition.worldPosition != previousPosition;
    }

    internal void LateUpdate()
    {
        deltaTime = Mathf.Min(deltaTimeCap, Time.deltaTime);

        if (transform.position.y < minimumYPosition)
        {
            SetPosition(characterPosition.worldPosition);
            return;
        }

        if (freeMovementController.IsActive())
        {
            velocity = freeMovementController.CalculateMovement();
        }
        else
        {
            velocity.x = 0f;
            velocity.z = 0f;
            velocity.y += gravity * deltaTime;

            bool previouslyGrounded = isGrounded;

            if (!isJumping || velocity.y <= 0f)
                CheckGround();

            if (isGrounded)
            {
                isJumping = false;
                velocity.y = gravity * deltaTime; // to avoid accumulating gravity in velocity.y while grounded
            }
            else if (previouslyGrounded && !isJumping)
            {
                lastUngroundedTime = Time.time;
            }

            if (Utils.isCursorLocked && characterForward.HasValue())
            {
                // Horizontal movement
                var speed = movementSpeed * (isSprinting ? runningSpeedMultiplier : 1f);

                transform.forward = characterForward.Get().Value;

                var xzPlaneForward = Vector3.Scale(cameraForward.Get(), new Vector3(1, 0, 1));
                var xzPlaneRight = Vector3.Scale(cameraRight.Get(), new Vector3(1, 0, 1));

                Vector3 forwardTarget = Vector3.zero;

                if (characterYAxis.GetValue() > 0)
                    forwardTarget += xzPlaneForward;
                if (characterYAxis.GetValue() < 0)
                    forwardTarget -= xzPlaneForward;

                if (characterXAxis.GetValue() > 0)
                    forwardTarget += xzPlaneRight;
                if (characterXAxis.GetValue() < 0)
                    forwardTarget -= xzPlaneRight;


                forwardTarget.Normalize();
                velocity += forwardTarget * speed;
                CommonScriptableObjects.playerUnityEulerAngles.Set(transform.eulerAngles);
            }

            bool jumpButtonPressedWithGraceTime = jumpButtonPressed && (Time.time - lastJumpButtonPressedTime < 0.15f);

            if (jumpButtonPressedWithGraceTime) // almost-grounded jump button press allowed time
            {
                bool justLeftGround = (Time.time - lastUngroundedTime) < 0.1f;

                if (isGrounded || justLeftGround) // just-left-ground jump allowed time
                {
                    Jump();
                }
            }

            //NOTE(Mordi): Detecting when the character hits the ground (for landing-SFX)
            if (isGrounded && !previouslyGrounded && (Time.time - lastUngroundedTime) > 0.4f)
            {
                OnHitGround?.Invoke();
            }
        }

        bool movingPlatformMovedTooMuch = Vector3.Distance(lastPosition, transform.position) > movingPlatformAllowedPosDelta;

        if (isOnMovingPlatform && !characterPosition.RepositionedWorldLastFrame() && movingPlatformMovedTooMuch)
        {
            ResetGround();
            // As the character has already been moved faster-than-we-want, we reposition it
            characterController.transform.position = lastPosition;
        }

        if (characterController.enabled)
        {
            //NOTE(Brian): Transform has to be in sync before the Move call, otherwise this call
            //             will reset the character controller to its previous position.
            Environment.i.platform.physicsSyncController.Sync();
            characterController.Move(velocity * deltaTime);
        }

        SetPosition(PositionUtils.UnityToWorldPosition(transform.position));

        if ((DCLTime.realtimeSinceStartup - lastMovementReportTime) > PlayerSettings.POSITION_REPORTING_DELAY)
        {
            ReportMovement();
        }

        if (isOnMovingPlatform)
        {
            lastLocalGroundPosition = groundTransform.InverseTransformPoint(transform.position);
        }

        OnUpdateFinish?.Invoke(deltaTime);
    }

    void Jump()
    {
        if (isJumping) return;

        isJumping = true;
        isGrounded = false;

        ResetGround();

        velocity.y = jumpForce;

        OnJump?.Invoke();
    }

    public void ResetGround()
    {
        if (isOnMovingPlatform)
            CommonScriptableObjects.playerIsOnMovingPlatform.Set(false);

        isOnMovingPlatform = false;
        groundTransform = null;
        movingPlatformSpeed = 0;
    }

    void CheckGround()
    {
        if (groundTransform == null)
            ResetGround();

        if (isOnMovingPlatform)
        {
            Physics.SyncTransforms();
            //NOTE(Brian): This should move the character with the moving platform
            Vector3 newGroundWorldPos = groundTransform.TransformPoint(lastLocalGroundPosition);
            movingPlatformSpeed = Vector3.Distance(newGroundWorldPos, transform.position);
            transform.position = newGroundWorldPos;
        }

        Transform transformHit = CastGroundCheckingRays();

        if (transformHit != null)
        {
            if (groundTransform == transformHit)
            {
                bool groundHasMoved = (transformHit.position != groundLastPosition || transformHit.rotation != groundLastRotation);

                if (supportsMovingPlatforms
                    && !characterPosition.RepositionedWorldLastFrame()
                    && groundHasMoved)
                {
                    isOnMovingPlatform = true;
                    CommonScriptableObjects.playerIsOnMovingPlatform.Set(true);
                    Physics.SyncTransforms();
                    lastLocalGroundPosition = groundTransform.InverseTransformPoint(transform.position);
                }
            }
            else
            {
                groundTransform = transformHit;
            }
        }
        else
        {
            ResetGround();
        }

        if (groundTransform != null)
        {
            groundLastPosition = groundTransform.position;
            groundLastRotation = groundTransform.rotation;
        }

        isGrounded = groundTransform != null && groundTransform.gameObject.activeInHierarchy;
    }

    // We secuentially cast rays in 4 directions (only if the previous one didn't hit anything)
    Transform CastGroundCheckingRays()
    {
        RaycastHit hitInfo;
        float rayMagnitude = (collider.bounds.extents.y + groundCheckExtraDistance);

        Ray ray = new Ray(transform.position, Vector3.down * rayMagnitude);
        if (!CastGroundCheckingRay(ray, Vector3.zero, out hitInfo, rayMagnitude) // center
            && !CastGroundCheckingRay(ray, transform.forward, out hitInfo, rayMagnitude) // forward
            && !CastGroundCheckingRay(ray, transform.right, out hitInfo, rayMagnitude) // right
            && !CastGroundCheckingRay(ray, -transform.forward, out hitInfo, rayMagnitude) // back
            && !CastGroundCheckingRay(ray, -transform.right, out hitInfo, rayMagnitude)) // left
        {
            return null;
        }

        // At this point there is a guaranteed hit, so this is not null
        return hitInfo.transform;
    }

    bool CastGroundCheckingRay(Ray ray, Vector3 originOffset, out RaycastHit hitInfo, float rayMagnitude)
    {
        ray.origin = transform.position + 0.9f * collider.bounds.extents.x * originOffset;
#if UNITY_EDITOR
        Debug.DrawRay(ray.origin, ray.direction, Color.red);
#endif
        return Physics.Raycast(ray, out hitInfo, rayMagnitude, groundLayers);
    }

    void ReportMovement()
    {
        float height = 0.875f;

        var reportPosition = characterPosition.worldPosition + (Vector3.up * height);
        var compositeRotation = Quaternion.LookRotation(cameraForward.Get());
        var playerHeight = height + (characterController.height / 2);

        //NOTE(Brian): We have to wait for a Teleport before sending the ReportPosition, because if not ReportPosition events will be sent
        //             When the spawn point is being selected / scenes being prepared to be sent and the Kernel gets crazy.

        //             The race conditions that can arise from not having this flag can result in:
        //                  - Scenes not being sent for loading, making ActivateRenderer never being sent, only in WSS mode.
        //                  - Random teleports to 0,0 or other positions that shouldn't happen.
        if (initialPositionAlreadySet)
            DCL.Interface.WebInterface.ReportPosition(reportPosition, compositeRotation, playerHeight);

        lastMovementReportTime = DCLTime.realtimeSinceStartup;
    }

    public void PauseGravity()
    {
        gravity = 0f;
        velocity.y = 0f;
    }

    public void ResumeGravity()
    {
        gravity = originalGravity;
    }

    void OnRenderingStateChanged(bool isEnable, bool prevState)
    {
        SetEnabled(isEnable);
    }
}