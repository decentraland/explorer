using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BuilderInWorldLoadingTipModel
{
    public string tipMessage;
    public Sprite tipImage;
}

public class BuilderInWorldLoadingTip : MonoBehaviour
{
    [SerializeField] internal TMP_Text tipText;
    [SerializeField] internal Image tipImage;

    public void Configure(BuilderInWorldLoadingTipModel tipModel)
    {
        tipText.text = tipModel.tipMessage;
        tipImage.sprite = tipModel.tipImage;
    }
}