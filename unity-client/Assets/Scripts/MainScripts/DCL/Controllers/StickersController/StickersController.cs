using UnityEngine;

public class StickersController : MonoBehaviour
{
    [SerializeField] private StickersFactory stickersFactory;

    public void PlayEmote(string id)
    {
        if (!stickersFactory.TryGet(id, out GameObject prefab))
            return;

        GameObject emoteGameObject = Instantiate(prefab);
        emoteGameObject.transform.position += transform.position;
        FollowObject emoteFollow = emoteGameObject.AddComponent<FollowObject>();
        emoteFollow.target = transform;
        emoteFollow.offset = prefab.transform.position;
    }
}