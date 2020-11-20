using UnityEngine;

namespace DCL.Rendering
{
    /// <summary>
    /// Group of arguments for configuring the rules of a group of renderers of skinned renderers.
    /// Used by CullingControllerSettings.
    /// </summary>
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
        /// Performs a linear interpolation between the values of two CullingControllerProfiles.
        /// Used for controlling the settings panel slider.
        /// </summary>
        /// <param name="p1">Starting profile</param>
        /// <param name="p2">Ending profile</param>
        /// <param name="t">Time value for the linear interpolation.</param>
        /// <returns>A new CullingControllerProfile with the interpolated values.</returns>
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

        /// <summary>
        /// Returns a clone of this object.
        /// </summary>
        /// <returns>The clone.</returns>
        public CullingControllerProfile Clone()
        {
            return this.MemberwiseClone() as CullingControllerProfile;
        }
    }
}