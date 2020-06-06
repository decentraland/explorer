using UnityEngine;

public class NFTShapeFactory : ScriptableObject
{
    [SerializeField] GameObject[] loaderControllersPrefabs;

    public GameObject InstantiateLoaderController(int index)
    {
        if (index >= 0 && index < loaderControllersPrefabs.Length)
        {
            return Object.Instantiate(loaderControllersPrefabs[index]);
        }
        return null;
    }
}
