using DCL;
using DCL.Controllers;
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

        int size = currentParcelScene.sceneData.parcels.Length * 2;
        int meters = size * 16;
        titleTxt.text = size + "x" + size + " LAND <color=#959696>" + meters+"x"+ meters+"m";


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
        return name + ":   " + usage + " / <color=#959696>" + limit + "</color>";
    }
}
