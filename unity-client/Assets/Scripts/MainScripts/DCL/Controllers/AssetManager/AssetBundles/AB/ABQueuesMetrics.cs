using DCL;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// This class is only for test purposes, it will be deleted...
public class ABQueuesMetrics : MonoBehaviour
{
    public RectTransform queue1Bar;
    public Text queue1Text;
    public RectTransform queue2Bar;
    public Text queue2Text;
    public Image queue2Image;

    private void Start()
    {
        AssetBundlesLoader.OnQueuesChanged += AssetBundlesLoader_OnQueuesChanged;
        RefreshMetrics(0, 0);
    }

    private void OnDestroy()
    {
        AssetBundlesLoader.OnQueuesChanged -= AssetBundlesLoader_OnQueuesChanged;
    }

    private void AssetBundlesLoader_OnQueuesChanged(System.Collections.Generic.KeyValuePair<int, int> obj)
    {
        RefreshMetrics(obj.Key, obj.Value);
    }

    private void RefreshMetrics(int queue1Value, int queue2Value)
    {
        queue1Bar.sizeDelta = new Vector2(queue1Bar.sizeDelta.x, queue1Value);
        queue1Text.text = queue1Value.ToString();
        queue2Bar.sizeDelta = new Vector2(queue2Bar.sizeDelta.x, queue2Value);
        queue2Text.text = queue2Value.ToString();
    }
}
