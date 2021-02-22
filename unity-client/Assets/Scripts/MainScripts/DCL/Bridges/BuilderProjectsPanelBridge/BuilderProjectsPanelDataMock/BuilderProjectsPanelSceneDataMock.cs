using System;
using UnityEngine;

[Serializable]
public struct BuilderProjectsPanelSceneDataMock
{
    public Vector2Int coords;
    public Vector2Int size;
    public string id;
    public string name;
    public string thumbnailUrl;
    public bool isOwner;
    public bool isOperator;
    public bool isContributor;
    public bool isDeployed;
}
