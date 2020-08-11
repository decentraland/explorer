using UnityEngine;

internal interface IExploreViewWithFriends
{
    void OnFriendAdded(UserProfile profile);
    void OnFriendRemoved(UserProfile profile);
    bool ContainCoords(Vector2Int coords);
}
