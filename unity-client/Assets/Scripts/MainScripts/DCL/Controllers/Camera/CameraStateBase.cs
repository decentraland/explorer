using UnityEngine;
public class CameraStateBase : MonoBehaviour
{
    public enum ModeId
    {
        FirstPerson,
        ThirdPerson,
    }

    protected Transform cameraTransform;
    public ModeId cameraModeId;

    public virtual void Init(Transform cameraTransform)
    {
        this.cameraTransform = cameraTransform;
        gameObject.SetActive(false);
    }

    public virtual void OnSelect()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnUnselect()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnSetRotation(CameraController.SetRotationPayload payload)
    {

    }
}
