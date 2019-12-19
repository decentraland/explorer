using Cinemachine;
using UnityEngine;

public class CameraStateTPS : CameraStateBase
{
    public CinemachineFreeLook defaultVirtualCamera;

    public LayerMask roofMask;

    [SerializeField] private InputAction_Measurable characterYAxis;
    [SerializeField] private InputAction_Measurable characterXAxis;

    protected Vector3Variable characterPosition => CommonScriptableObjects.playerUnityPosition;
    protected Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;
    protected Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
    protected Vector3Variable cameraRight => CommonScriptableObjects.cameraRight;
    protected Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;
    protected Vector3Variable playerUnityToWorldOffset => CommonScriptableObjects.playerUnityToWorldOffset;

    public float rotationLerpSpeed = 10;

    public override void OnSelect()
    {
        if (characterForward.Get().HasValue)
        {
            defaultVirtualCamera.m_XAxis.Value = Quaternion.LookRotation(characterForward.Get().Value, Vector3.up).eulerAngles.y;
            defaultVirtualCamera.m_YAxis.Value = 0.5f;
        }

        base.OnSelect();
    }

    public override void OnUpdate()
    {
        defaultVirtualCamera.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;

        var xzPlaneForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
        var xzPlaneRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));

        if (characterYAxis.GetValue() != 0f || characterXAxis.GetValue() != 0f)
        {
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

            if (!characterForward.HasValue())
            {
                characterForward.Set(forwardTarget);
            }
            else
            {
                var lerpedForward = Vector3.Slerp(characterForward.Get().Value, forwardTarget, rotationLerpSpeed * Time.deltaTime);
                characterForward.Set(lerpedForward);
            }
        }
    }

    public override void OnSetRotation(CameraController.SetRotationPayload payload)
    {
        var eulerDir = Vector3.zero;

        if (payload.cameraTarget.HasValue)
        {
            var newPos = new Vector3(payload.x, payload.y, payload.z);
            var cameraTarget = payload.cameraTarget.GetValueOrDefault();
            var dirToLook = (cameraTarget - newPos);
            eulerDir = Quaternion.LookRotation(dirToLook).eulerAngles;
        }

        defaultVirtualCamera.m_XAxis.Value = eulerDir.y;
        defaultVirtualCamera.m_YAxis.Value = eulerDir.x;
    }
}
