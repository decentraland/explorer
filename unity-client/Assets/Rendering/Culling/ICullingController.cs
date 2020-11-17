namespace DCL.Rendering
{
    public interface ICullingController
    {
        void Start();
        void Stop();
        event CullingController.DataReport OnDataReport;
        void SetDirty();

        void SetSettings(CullingControllerSettings settings);
        CullingControllerSettings GetSettings();

        void SetObjectCulling(bool enabled);
        void SetAnimationCulling(bool enabled);
        void SetShadowCulling(bool enabled);
    }
}