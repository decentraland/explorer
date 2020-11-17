namespace DCL.Rendering
{
    [System.Serializable]
    public class CullingControllerSettings
    {
        public bool enableObjectCulling = true;
        public bool enableShadowCulling = true;
        public bool enableAnimationCulling = true;

        public float enableAnimationCullingDistance = 7.5f;

        public CullingControllerProfile rendererProfile =
            new CullingControllerProfile
            {
                visibleDistanceThreshold = 30,
                shadowDistanceThreshold = 20,
                emissiveSizeThreshold = 2.5f,
                opaqueSizeThreshold = 6,
                shadowRendererSizeThreshold = 10,
                shadowMapProjectionSizeThreshold = 4
            };

        public CullingControllerProfile skinnedRendererProfile =
            new CullingControllerProfile
            {
                visibleDistanceThreshold = 50,
                shadowDistanceThreshold = 40,
                emissiveSizeThreshold = 2.5f,
                opaqueSizeThreshold = 6,
                shadowRendererSizeThreshold = 5,
                shadowMapProjectionSizeThreshold = 4,
            };
    }
}