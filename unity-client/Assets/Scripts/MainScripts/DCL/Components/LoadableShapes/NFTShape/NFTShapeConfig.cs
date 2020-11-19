using UnityEngine;

public class NFTShapeConfig : ScriptableObject
{
    public float loadingMinDistance = 30;
    public float highQualityImageMinDistance = 3;
    public float highQualityImageAngleRatio = 0.8f; //Only used when shape doesn't contain a collider
    public int highQualityImageResolution = 1024;
    public bool verbose = false;
}
