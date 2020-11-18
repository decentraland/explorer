using UnityEngine;

namespace DCL.Rendering
{
    [System.Serializable]
    public class CullingControllerProfile
    {
        public float visibleDistanceThreshold;
        public float shadowDistanceThreshold;

        public float emissiveSizeThreshold;
        public float opaqueSizeThreshold;
        public float shadowRendererSizeThreshold;
        public float shadowMapProjectionSizeThreshold;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static CullingControllerProfile Lerp(CullingControllerProfile p1, CullingControllerProfile p2, float t)
        {
            return new CullingControllerProfile
            {
                visibleDistanceThreshold = Mathf.Lerp(p1.visibleDistanceThreshold, p2.visibleDistanceThreshold, t),
                shadowDistanceThreshold = Mathf.Lerp(p1.shadowDistanceThreshold, p2.shadowDistanceThreshold, t),
                emissiveSizeThreshold = Mathf.Lerp(p1.emissiveSizeThreshold, p2.emissiveSizeThreshold, t),
                opaqueSizeThreshold = Mathf.Lerp(p1.opaqueSizeThreshold, p2.opaqueSizeThreshold, t),
                shadowRendererSizeThreshold = Mathf.Lerp(p1.shadowRendererSizeThreshold, p2.shadowRendererSizeThreshold, t),
                shadowMapProjectionSizeThreshold = Mathf.Lerp(p1.shadowMapProjectionSizeThreshold, p2.shadowMapProjectionSizeThreshold, t)
            };
        }

        public CullingControllerProfile Clone()
        {
            return this.MemberwiseClone() as CullingControllerProfile;
        }
    }
}