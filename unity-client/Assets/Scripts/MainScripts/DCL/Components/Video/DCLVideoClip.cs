using System.Collections;
using DCL.Controllers;

namespace DCL.Components
{
    public class DCLVideoClip : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string url;
        }

        public Model model;

        public DCLVideoClip(ParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);
            yield break;
        }

        public string GetUrl()
        {
            string contentsUrl = string.Empty;
            bool isExternalURL = model.url.StartsWith("http://") || model.url.StartsWith("https://");

            if (isExternalURL)
                contentsUrl = model.url;
            else
                scene.contentProvider.TryGetContentsUrl(model.url, out contentsUrl);
            return contentsUrl;
        }
    }
}
