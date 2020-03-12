using DCL;
using DCL.Helpers;
using UnityEngine;

//(Alex) In the future explorer could provide non-existent parcels as well, we could then remove this and simply add a basic logic to change the color of a representation in ParcelScene
public class MinimapParcelsView : MonoBehaviour
{
    private Vector2IntVariable playerCoords => CommonScriptableObjects.playerCoords;
    private Vector3Variable playerWorldPosition => CommonScriptableObjects.playerWorldPosition;

    public MinimapAtlas atlas;

    public void Start()
    {
        playerCoords.OnChange += OnCharacterSetPosition;
        playerWorldPosition.OnChange += OnCharacterMove;
        UpdateMinimapAtlas(Vector2Int.zero);
    }

    public void OnDestroy()
    {
        playerCoords.OnChange -= OnCharacterSetPosition;
        playerWorldPosition.OnChange -= OnCharacterMove;
    }

    private void OnCharacterMove(Vector3 current, Vector3 previous)
    {
        UpdateMinimapAtlas(Utils.WorldToGridPositionUnclamped(current));
    }

    public void OnCharacterSetPosition(Vector2Int newCoords, Vector2Int oldCoords)
    {
        UpdateMinimapAtlas(new Vector2((float)newCoords.x, (float)newCoords.y));
    }

    public void UpdateMinimapAtlas(Vector2 newCoords)
    {
        atlas.CenterToTile(newCoords);
    }
}
