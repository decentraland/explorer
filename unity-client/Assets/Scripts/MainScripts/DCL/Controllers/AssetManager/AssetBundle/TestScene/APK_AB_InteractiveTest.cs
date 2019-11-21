using DCL;
using System.Collections.Generic;
using UnityEngine;

public class APK_AB_InteractiveTest : MonoBehaviour
{
    AssetPromiseKeeper_AssetBundleModel keeper;
    AssetLibrary_AssetBundleModel library;

    List<AssetPromise_AssetBundleModel> promiseList = new List<AssetPromise_AssetBundleModel>();

    void Start()
    {
        library = new AssetLibrary_AssetBundleModel();
        keeper = new AssetPromiseKeeper_AssetBundleModel(library);
    }

    void Generate(string url, string hash)
    {
        AssetPromise_AssetBundleModel promise = new AssetPromise_AssetBundleModel("http://localhost:1338/", url);

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
