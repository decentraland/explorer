using System;

public static class CatalystEntitiesType
{
    public static readonly string SCENE = "scene";
    public static readonly string PROFILE = "profile";
    public static readonly string WEARABLE = "wearable";
}

public static class CatalystSortingField
{
    public static readonly string LOCAL_TIMESTAMP = "local_timestamp";
    public static readonly string ENTITY_TIMESTAMP = "entity_timestamp";
}

public static class CatalystSortingOrder
{
    public static readonly string ASCENDING = "ASC";
    public static readonly string DESCENDING = "DESC";
}

[Serializable]
public class DeploymentFilters
{
    public bool onlyCurrentlyPointed;
    public string[] pointers;
    public string[] entityIds;
    public string[] entityTypes;
    public string[] deployedBy;
}

[Serializable]
public class DeploymentOptions 
{
    public DeploymentFilters filters;
    public string sortBy;
    public string sortOrder;
    public int? offset;
    public int? limit;
    public string lastId;
}

[Serializable]
public class SceneDeploymentPayload
{
    public DeploymentScene[] deployments;
    public DeploymentFilters filters;
}

[Serializable]
public class DeploymentBase
{
    public string entityType;
    public string entityId;
    public ulong entityTimestamp;
    public string deployedBy;
    public string[] pointers;
    public DeploymentContent[] content;
}

[Serializable]
public class DeploymentContent
{
    public string key;
    public string hash;
}

[Serializable]
public class DeploymentScene : DeploymentBase
{
    public DeploymentSceneMetadata metadata;
}

[Serializable]
public class DeploymentSceneMetadata
{
    [Serializable]
    public class Display
    {
        public string title;
        public string description;
        public string navmapThumbnail;
    }
    
    [Serializable]
    public class Contact
    {
        public string name;
    }
    
    [Serializable]
    public class Scene
    {
        public string @base;
        public string[] parcels;
    }
    
    [Serializable]
    public class Policy
    {
        public string contentRating;
        public bool fly;
        public bool voiceEnabled;
        public string[] blacklist;
    }
    
    [Serializable]
    public class SpawnPoint
    {
        [Serializable]
        public class Vector3
        {
            public float x;
            public float y;
            public float z;
        }

        public string name;
        public bool @default;
        public Vector3 position;
        public Vector3 cameraTarget;
    }

    public Display display;
    public Contact contact;
    public Scene scene;
    public Policy policy;
    public SpawnPoint[] spawnPoints;
    public string owner;
    public string[] tags;
}