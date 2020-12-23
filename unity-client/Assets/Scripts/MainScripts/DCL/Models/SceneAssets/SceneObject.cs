using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SceneObject 
{
    [System.Serializable]
    public class Action
    {
        public string id;
        public string label;
        public Parameter[] parameters;
    }
    [System.Serializable]
    public class Parameter
    {
        public string id;
        public string label;
        public string type;
        public string defaultValue;
        public string min;
        public string max;
        public string step;
        public OptionsParameter[] options;

        [System.Serializable]
        public class OptionsParameter
        {
            public string label;
            public string value;
        }
    }

    [System.Serializable]
    public class ObjectMetrics
    {
        public int meshes;
        public int bodies;
        public int materials;
        public int textures;
        public int triangles;
        public int entities;
    }


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
    public Parameter[] parameters;
    public Action[] actions;
    public string script;
    public bool isFavorite = false;

    string baseUrl = "https://builder-api.decentraland.org/v1/storage/contents/";
    public string GetComposedThumbnailUrl()
    {
        return baseUrl + thumbnail;
    }
}