using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SceneObject 
{

    [System.Serializable]
    public class ObjectMetrics
    {
        public int meshes;
        public int bodies;
        public int materials;
        public int textures;
        public int triangles;
        public int entitites;
    }

    //[System.Serializable]
    //public class Contents
    //{
    //    public Dictionary<string, string> contentValues;
    //}


    public string id;
    public string asset_pack_id;
    public string name;
    public string model;
    public string thumbnail;
    public List<string> tags;

    public string category;
    public Dictionary<string, string> contents;

    public string created_at;
    public string updated_at;

    public ObjectMetrics metrics;
    public string script;
    //public List<string> parameters;
    //public List<string> actions;


}