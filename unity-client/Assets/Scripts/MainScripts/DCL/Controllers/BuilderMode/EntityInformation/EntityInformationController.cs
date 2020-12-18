using DCL;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityInformationController : MonoBehaviour
{
    [Header("Prefab references")]
    public TextMeshProUGUI titleTxt;
    public RawImage entitytTumbailImg; 
    public AttributeXYZ positionAttribute;

    DCLBuilderInWorldEntity currentEntity;
    ParcelScene parcelScene;

    bool isEnable = false;

    int framesBetweenUpdate = 5;
    int framesCount = 0;

    string loadedThumbnailURL;
    AssetPromise_Texture loadedThumbnailPromise;

    private void LateUpdate()
    {
        if (!isEnable) return;

        if (currentEntity == null) return;

        if (framesCount >= framesBetweenUpdate)
        {
            UpdateInfo();
            framesCount = 0;
        }
        else
        {
            framesCount++;
        }
    }

    public void SetEntity(DCLBuilderInWorldEntity entity, ParcelScene currentScene)
    {
        this.currentEntity = entity;
        parcelScene = currentScene;
        titleTxt.text = entity.GetDescriptiveName();

        GetThumbnail(entity.GetSceneObjectAssociated());

        UpdateInfo();
    }

    public void Enable()
    {
        gameObject.SetActive(true);
        isEnable = true;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        isEnable = false;
    }

    public void UpdateInfo()
    {
        if (currentEntity.gameObject != null)
        {
            Vector3 positionConverted = Environment.i.worldState.ConvertUnityToScenePosition(currentEntity.gameObject.transform.position, parcelScene);
            Vector3 currentRotation = currentEntity.gameObject.transform.rotation.eulerAngles;
            Vector3 currentScale = currentEntity.gameObject.transform.localScale;

            positionAttribute.SetValues(positionConverted);

            string desc = AppendUsageAndLimit("POSITION:   ", positionConverted, "0.#");
            desc += "\n\n" + AppendUsageAndLimit("ROTATION:  ", currentRotation, "0");
            desc += "\n\n" + AppendUsageAndLimit("SCALE:        ", currentScale, "0.##");

        }
    }

    string AppendUsageAndLimit(string name, Vector3 currentVector, string format)
    {
        return $"{name}X: {currentVector.x.ToString(format)}  Y: {currentVector.y.ToString(format)}  Z:{currentVector.z.ToString(format)}";
    }

    private void GetThumbnail(SceneObject sceneObject)
    {
        var url = sceneObject?.GetComposedThumbnailUrl();

        if (url == loadedThumbnailURL)
            return;

        if (sceneObject == null || string.IsNullOrEmpty(url))
            return;

        string newLoadedThumbnailURL = url;
        var newLoadedThumbnailPromise = new AssetPromise_Texture(url);


        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading: {url}"); };

        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);


        AssetPromiseKeeper_Texture.i.Forget(loadedThumbnailPromise);
        loadedThumbnailPromise = newLoadedThumbnailPromise;
        loadedThumbnailURL = newLoadedThumbnailURL;
    }

    public void SetThumbnail(Asset_Texture texture)
    {
        if (entitytTumbailImg != null)
        {
            entitytTumbailImg.enabled = true;
            entitytTumbailImg.texture = texture.texture;
        }
    }
}