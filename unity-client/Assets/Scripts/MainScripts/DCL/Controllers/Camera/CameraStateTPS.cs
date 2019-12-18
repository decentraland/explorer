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

    public override void OnSelect()
    {
        tpsInteriors.m_Transitions.m_InheritPosition = false;
        tpsDefault.m_Transitions.m_InheritPosition = false;
        tpsDefault.m_BindingMode = CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp;

        if (characterForward.Get().HasValue)
        {
            Vector3 v = characterPosition.Get() - (characterForward.Get().Value * 20);
            tpsInteriors.transform.position = v;
            tpsDefault.transform.position = v;
        }

        base.OnSelect();
    }

    public override void OnUnselect()
    {
        base.OnUnselect();
    }


    public override void OnUpdate()
    {
        tpsDefault.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;

        var xzPlaneForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
        var xzPlaneRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));

        if (Physics.SphereCast(characterPosition.Get(), 0.5f, Vector3.up * 5, out RaycastHit hitinfo, 5, roofMask) && hitinfo.normal.y < 0)
        {
            if (!tpsInteriors.gameObject.activeSelf)
            {
                tpsInteriors.m_Transitions.m_InheritPosition = true;
                tpsInteriors.gameObject.SetActive(true);
                tpsDefault.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!tpsDefault.gameObject.activeSelf)
            {
                tpsDefault.m_Transitions.m_InheritPosition = true;
                tpsDefault.gameObject.SetActive(true);
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

            //if (!characterForward.HasValue())
            //{
            characterForward.Set(forwardTarget);
            //}
            //else
            //{
            //    var lerpedForward = Vector3.Slerp(characterForward.Get().Value, forwardTarget, rotationLerpSpeed * Time.deltaTime);
            //    characterForward.Set(lerpedForward);
            //}
        }
    }
}
