using Cinemachine;
using UnityEngine;

public class CameraStateTPS : CameraStateBase
{
    public CinemachineFreeLook tpsInteriors;
    public CinemachineFreeLook tpsDefault;
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


    public override void OnUpdate()
    {
        var xzPlaneForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
        var xzPlaneRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));

        if (CinemachineCore.Instance.GetActiveBrain(0).IsBlending)
        {
            tpsInteriors.m_YAxis.Value = 0.5f;
            tpsDefault.m_YAxis.Value = 0.5f;
            tpsInteriors.m_XAxis.Value = 0f;
            tpsDefault.m_XAxis.Value = 0f;
        }

        if (Physics.SphereCast(characterPosition.Get(), 2.0f, Vector3.up * 5, out RaycastHit hitinfo, 5, roofMask) && hitinfo.normal.y < 0)
        {
            if (!tpsInteriors.gameObject.activeSelf)
            {
                tpsInteriors.gameObject.SetActive(true);
                //tpsInteriors.m_XAxis.Value = tpsDefault.m_XAxis.Value;
                //tpsInteriors.m_YAxis.Value = tpsDefault.m_YAxis.Value;
                tpsDefault.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!tpsDefault.gameObject.activeSelf)
            {
                tpsDefault.gameObject.SetActive(true);
                //tpsDefault.m_XAxis.Value = tpsInteriors.m_XAxis.Value;
                //tpsDefault.m_YAxis.Value = tpsInteriors.m_YAxis.Value;
                tpsInteriors.gameObject.SetActive(false);
            }
        }

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
}
