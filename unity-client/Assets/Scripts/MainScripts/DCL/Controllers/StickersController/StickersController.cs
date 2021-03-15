using UnityEngine;

public class StickersController : MonoBehaviour
{
    [SerializeField] private StickersFactory stickersFactory;

    public void PlayEmote(string id)
    {
        if (!stickersFactory.TryGet(id, out GameObject prefab))
            return;

        var emoteGameObject = Instantiate(prefab);
        emoteGameObject.AddComponent<>()
    }
}