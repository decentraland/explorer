using DCL;
using System.Collections.Generic;
using UnityEngine;

public class APK_AB_InteractiveTest : MonoBehaviour
{
    ContentProvider_Dummy provider;
    AssetPromiseKeeper_AssetBundle keeper;
    AssetLibrary_AssetBundle library;

    List<AssetPromise_AssetBundle> promiseList = new List<AssetPromise_AssetBundle>();

    void Start()
    {
        provider = new ContentProvider_Dummy();
        library = new AssetLibrary_AssetBundle();
        keeper = new AssetPromiseKeeper_AssetBundle(library);
    }

    void Generate(string url, string hash)
    {
        AssetPromise_AssetBundle promise = new AssetPromise_AssetBundle(provider, url);

        if (!provider.fileToHash.ContainsKey(url.ToLower()))
            provider.fileToHash.Add(url.ToLower(), hash);

        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-10, 10);
        pos.z = Random.Range(-10, 10);
        promise.settings.initialLocalPosition = pos;

        keeper.Keep(promise);
        promiseList.Add(promise);
    }
    static int counter = 0;
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Z))
        {
            counter++;
            counter %= 3;
            switch (counter)
            {
                case 0:
                    string url = "http://localhost:1338/QmZFSGh3KYXC4hnjjUvdnqE1owraaMcFXny8oL6ctHR47L";
                    Generate(url, "QmZFSGh3KYXC4hnjjUvdnqE1owraaMcFXny8oL6ctHR47L");
                    break;
                case 1:
                    string url2 = "http://localhost:1338/QmZFSGh3KYXC4hnjjUvdnqE1owraaMcFXny8oL6ctHR47L";
                    Generate(url2, "QmZFSGh3KYXC4hnjjUvdnqE1owraaMcFXny8oL6ctHR47L");
                    break;
                case 2:
                    string url3 = "http://localhost:1338/QmZFSGh3KYXC4hnjjUvdnqE1owraaMcFXny8oL6ctHR47L";
                    Generate(url3, "QmZFSGh3KYXC4hnjjUvdnqE1owraaMcFXny8oL6ctHR47L");
                    break;
            }

        }
        else if (Input.GetKeyUp(KeyCode.X))
        {
            if (promiseList.Count > 0)
            {
                var promiseToRemove = promiseList[Random.Range(0, promiseList.Count)];
                keeper.Forget(promiseToRemove);
                promiseList.Remove(promiseToRemove);
            }
        }

    }
}
