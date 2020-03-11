using UnityEngine;

//(Alex) In the future explorer could provide non-existent parcels as well, we could then remove this and simply add a basic logic to change the color of a representation in ParcelScene
public class MinimapParcelsView : MonoBehaviour
{
    private Vector2IntVariable playerCoords => CommonScriptableObjects.playerCoords;
    private Vector3Variable playerUnityToWorldOffset => CommonScriptableObjects.playerUnityToWorldOffset;
    private MinimapMetadata minimapmetadata => MinimapMetadata.GetMetadata();

    private Vector3Variable.Change unityWorldOffsetDelegate;

    private void Start()
    {
        playerCoords.OnChange += OnCharacterSetPosition;
        DrawParcels(Vector2Int.zero);
    }


    public void OnCharacterSetPosition(Vector2Int newCoords, Vector2Int oldCoords)
    {
        DrawParcels(newCoords);
    }

    public void DrawParcels(Vector2Int newCoords)
    {
    }


    private void OnDestroy()
    {
        playerCoords.OnChange -= OnCharacterSetPosition;
        playerUnityToWorldOffset.OnChange -= unityWorldOffsetDelegate;
    }
}
