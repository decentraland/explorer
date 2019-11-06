﻿using DCL.Controllers;

namespace DCL.Components
{
    public class GLTFShape : LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>
    {
        public override string componentName => "GLTF Shape";

        public GLTFShape(ParcelScene scene) : base(scene)
        {
        }

        public override string ToString()
        {
            string fullUrl;

            bool found = scene.contentProvider.TryGetContentsUrl(model.src, out fullUrl);

            if (!found)
                fullUrl = "Not found!";

            return $"{componentName} (src = {model.src}, full url = {fullUrl}";
        }
    }
}
