using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public class GlobalScene : ParcelScene
    {
        [System.NonSerialized]
        public bool isPortableExperience = false;

        [System.NonSerialized]
        public string iconUrl;

        protected override string prettyName => $"{sceneData.id}{ (isPortableExperience ? " (PE)" : "") }";

        public override bool IsInsideSceneBoundaries(Vector3 worldPosition, float height = 0f)
        {
            return true;
        }

        public override bool IsInsideSceneBoundaries(Vector2Int gridPosition, float height = 0)
        {
            return true;
        }

        public override void SetData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            this.sceneData = data;

            contentProvider = new ContentProvider_Dummy();
            contentProvider.baseUrl = data.baseUrl;
        }

        protected override void SendMetricsEvent()
        {
        }
    }
}
