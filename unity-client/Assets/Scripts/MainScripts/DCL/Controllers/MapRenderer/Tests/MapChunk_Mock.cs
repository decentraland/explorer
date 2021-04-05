using UnityEngine.Networking;

namespace DCL
{
    public class MapChunk_Mock : MapChunk
    {
        public override UnityWebRequestAsyncOperation LoadChunkImage()
        {
            isLoadingOrLoaded = true;

            return new UnityWebRequestAsyncOperation();
        }
    }
}