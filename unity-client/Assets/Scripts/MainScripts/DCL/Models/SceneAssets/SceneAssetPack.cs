using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAssetPack 
{
    public string id;
    public string title;
    public string thumbnail;
    public string user_id;
    public string created_at;
    public string updated_at;
    public string eth_address;
    public List<SceneObject> assets;



    //public void LoadSceneObjectListFromJSON(JObject jObject)
    //{
    //    assets = new List<SceneObject>();
    //    JArray array = JArray.Parse(jObject["assets"].ToString());
    //    //JArray array = new JArray(jObject["data"]);
    //    foreach (JObject item in array)
    //    {
    //        try
    //        {
    //            SceneObject sceneObject = JsonConvert.DeserializeObject<SceneObject>(item.ToString());
    //            assets.Add(sceneObject);
    //        }
    //        catch(Exception e)
    //        {
    //            SceneObject sceneObject = JsonConvert.DeserializeObject<SceneObject>(item.ToString());
    //            assets.Add(sceneObject);
    //        }
    //    }
    //}
}
