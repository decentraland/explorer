using DCL;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class MapRenderer : MonoBehaviour
{
    private Vector3Variable playerWorldPosition => CommonScriptableObjects.playerWorldPosition;
    private Vector3Variable playerRotation => CommonScriptableObjects.playerUnityEulerAngles;

    public MapAtlas atlas;

    public Image playerPositionIcon;

    public void Start()
    {
        playerWorldPosition.OnChange += OnCharacterMove;
        playerRotation.OnChange += OnCharacterRotate;
        UpdateMinimapAtlas(Vector2Int.zero);
    }

    public void OnDestroy()
    {
        playerWorldPosition.OnChange -= OnCharacterMove;
    }

    private void OnCharacterMove(Vector3 current, Vector3 previous)
    {
        UpdateMinimapAtlas(Utils.WorldToGridPositionUnclamped(current));
    }

    private void OnCharacterRotate(Vector3 current, Vector3 previous)
    {
        UpdateMinimapAtlas(Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get()));
    }

    public void OnCharacterSetPosition(Vector2Int newCoords, Vector2Int oldCoords)
    {
        UpdateMinimapAtlas(new Vector2((float)newCoords.x, (float)newCoords.y));
    }

    public void UpdateMinimapAtlas(Vector2 newCoords)
    {
        atlas.CenterToTile(newCoords);
        playerPositionIcon.transform.SetPositionAndRotation(atlas.GetViewportCenter(), Quaternion.Euler(0, 0, playerRotation.Get().y));

    }
}
