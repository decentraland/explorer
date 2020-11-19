using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFTShapeConfig : ScriptableObject
{
    public float loadingMinDistance = 30;
    public float highQualityImageMinDistance = 30;
    public float highQualityImageAngleRatio = 0;
    public int highQualityImageResolution = 1024;
    public bool verbose = false;
}
