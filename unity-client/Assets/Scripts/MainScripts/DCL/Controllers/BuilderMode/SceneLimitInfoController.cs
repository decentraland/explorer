using DCL;
using DCL.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SceneLimitInfoController : MonoBehaviour
{
    [Header("Scene references")]

    public TextMeshProUGUI titleTxt;
    public TextMeshProUGUI descTxt;

    ParcelScene currentParcelScene;


    public void SetParcelScene(ParcelScene _parcelScene)
    {
        currentParcelScene = _parcelScene;
        UpdateInfo();
    }
    public void Enable()
    {
        gameObject.SetActive(true);
        UpdateInfo();
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }


    public void UpdateInfo()
    {

  
        if (IsParcelSceneSquare(currentParcelScene))
        {
            int size = (int) Math.Sqrt(currentParcelScene.sceneData.parcels.Length);
            int meters = size * 16;
            titleTxt.text = size + "x" + size + " LAND <color=#959696>" + meters + "x" + meters + "m";
        }
        else titleTxt.text = "CUSTOM LAND";


        SceneMetricsController.Model limits = currentParcelScene.metricsController.GetLimits();
        SceneMetricsController.Model usage = currentParcelScene.metricsController.GetModel();

        string desc = AppendUsageAndLimit("TRIANGLES", usage.triangles, limits.triangles);
        desc += "\n" + AppendUsageAndLimit("MATERIALS", usage.materials, limits.materials);
        desc += "\n" + AppendUsageAndLimit("MESHES", usage.meshes, limits.meshes);
        desc += "\n" + AppendUsageAndLimit("BODIES", usage.bodies, limits.bodies);
        desc += "\n" + AppendUsageAndLimit("ENTITIES", usage.entities, limits.entities);
        desc += "\n" + AppendUsageAndLimit("TEXTURES", usage.textures, limits.textures);
        descTxt.text = desc;
    }


    string AppendUsageAndLimit(string name, int usage, int limit)
    {
        string currentString = name + ":   " + usage + " / <color=#959696>" + limit + "</color>";
        if (usage >= limit) currentString = "<color=red>" + currentString + "</color>";
        return currentString;
    }

    bool IsParcelSceneSquare(ParcelScene scene)
    {
        Vector2Int[] parcelsPoints = scene.sceneData.parcels;
        int minX = 9999;
        int minY = 9999;
        int maxX = -9999;
        int maxY = -9999;

        foreach(Vector2Int vector in parcelsPoints)
        {
            if (vector.x < minX) minX = vector.x;
            if (vector.y < minY) minY = vector.y;
            if (vector.x > maxX) maxX = vector.x;
            if (vector.y > maxY) maxY = vector.y;
        }

        if(maxX - minX != maxY - minY) return false;

        int lateralLengh = Math.Abs((maxX - minX) + 1);
        if (parcelsPoints.Length != lateralLengh * lateralLengh) return false;
        
        return true;
    }
}
