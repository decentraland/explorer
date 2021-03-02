using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using System;
using UnityEngine;

public interface IEntityInformationController
{
    event Action<Vector3> OnPositionChange;
    event Action<Vector3> OnRotationChange;
    event Action<Vector3> OnScaleChange;
    event Action<DCLBuilderInWorldEntity, string> OnNameChange;
    event Action<DCLBuilderInWorldEntity> OnSmartItemComponentUpdate;

    void Initialize(IEntityInformationView view);
    void Dispose();
    void PositionChanged(Vector3 pos);
    void RotationChanged(Vector3 rot);
    void ScaleChanged(Vector3 scale);
    void NameChanged(DCLBuilderInWorldEntity entity, string name);
    void ToggleDetailsInfo();
    void ToggleBasicInfo();
    void StartChangingName();
    void EndChangingName();
    void SetEntity(DCLBuilderInWorldEntity entity, ParcelScene currentScene);
    void Enable();
    void Disable();
    void UpdateInfo(DCLBuilderInWorldEntity entity);
}

public class EntityInformationController : IEntityInformationController
{
    public event Action<Vector3> OnPositionChange;
    public event Action<Vector3> OnRotationChange;
    public event Action<Vector3> OnScaleChange;
    public event Action<DCLBuilderInWorldEntity, string> OnNameChange;
    public event Action<DCLBuilderInWorldEntity> OnSmartItemComponentUpdate;

    private IEntityInformationView entityInformationView;
    private ParcelScene parcelScene;
    private AssetPromise_Texture loadedThumbnailPromise;
    private bool isChangingName = false;

    public void Initialize(IEntityInformationView entityInformationView)
    {
        this.entityInformationView = entityInformationView;

        entityInformationView.position.OnChanged += PositionChanged;
        entityInformationView.rotation.OnChanged += RotationChanged;
        entityInformationView.scale.OnChanged += ScaleChanged;
        entityInformationView.OnNameChange += NameChanged;
        entityInformationView.OnStartChangingName += StartChangingName;
        entityInformationView.OnEndChangingName += EndChangingName;
        entityInformationView.OnDisable += Disable;
        entityInformationView.OnUpdateInfo += UpdateInfo;
    }

    public void Dispose()
    {
        entityInformationView.position.OnChanged -= PositionChanged;
        entityInformationView.rotation.OnChanged -= RotationChanged;
        entityInformationView.scale.OnChanged -= ScaleChanged;
        entityInformationView.OnNameChange -= NameChanged;
        entityInformationView.OnUpdateInfo -= UpdateInfo;
        entityInformationView.OnStartChangingName -= StartChangingName;
        entityInformationView.OnEndChangingName -= EndChangingName;
        entityInformationView.OnDisable -= Disable;
    }

    public void PositionChanged(Vector3 pos)
    {
        OnPositionChange?.Invoke(pos);
    }

    public void RotationChanged(Vector3 rot)
    {
        OnRotationChange?.Invoke(rot);
    }

    public void ScaleChanged(Vector3 scale)
    {
        OnScaleChange?.Invoke(scale);
    }

    public void NameChanged(DCLBuilderInWorldEntity entity, string name)
    {
        OnNameChange?.Invoke(entity, name);
    }

    public void ToggleDetailsInfo()
    {
        entityInformationView.ToggleDetailsInfo();
    }

    public void ToggleBasicInfo()
    {
        entityInformationView.ToggleBasicInfo();
    }

    public void StartChangingName()
    {
        isChangingName = true;
    }

    public void EndChangingName()
    {
        isChangingName = false;
    }

    public void SetEntity(DCLBuilderInWorldEntity entity, ParcelScene currentScene)
    {
        EntityDeselected();

        if (entityInformationView.currentEntity != null)
            entity.onStatusUpdate -= UpdateEntityName;

        entityInformationView.SetCurrentEntity(entity);
        entityInformationView.currentEntity.onStatusUpdate += UpdateEntityName;
        parcelScene = currentScene;

        if (entity.HasSmartItemComponent())
        {
            if (entity.rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out BaseComponent baseComponent))
                entityInformationView.smartItemList.SetSmartItemParameters(entity.GetSmartItemParameters(), ((SmartItemComponent)baseComponent).model.values);
        }
        else
        {
            entityInformationView.SetSmartItemListViewActive(false);
        }

        entityInformationView.SetEntityThumbnailEnable(false);
        CatalogItem entitySceneObject = entity.GetCatalogItemAssociated();
        GetThumbnail(entitySceneObject);
        UpdateLimitsInformation(entitySceneObject);
        UpdateEntityName(entityInformationView.currentEntity);
        UpdateInfo(entityInformationView.currentEntity);
    }

    internal void GetThumbnail(CatalogItem catalogItem)
    {
        var url = catalogItem.thumbnailURL;

        if (catalogItem == null || string.IsNullOrEmpty(url))
            return;

        string newLoadedThumbnailURL = url;
        var newLoadedThumbnailPromise = new AssetPromise_Texture(url);
        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading: {url}"); };
        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);
        AssetPromiseKeeper_Texture.i.Forget(loadedThumbnailPromise);
        loadedThumbnailPromise = newLoadedThumbnailPromise;
    }

    internal void SetThumbnail(Asset_Texture texture)
    {
        entityInformationView.SetEntityThumbnailEnable(true);
        entityInformationView.SetEntityThumbnailTexture(texture.texture);
    }

    internal void UpdateEntityName(DCLBuilderInWorldEntity entity)
    {
        string currentName = entity.GetDescriptiveName();
        entityInformationView.SeTitleText(currentName);

        if (!isChangingName)
            entityInformationView.SetNameIFText(currentName);
    }

    internal void UpdateLimitsInformation(CatalogItem catalogItem)
    {
        if (catalogItem == null)
        {
            entityInformationView.SeEntityLimitsLeftText("");
            entityInformationView.SeEntityLimitsRightText("");
            return;
        }

        string leftText = $"ENTITIES: {catalogItem.metrics.entities}\n" +
                          $"BODIES: {catalogItem.metrics.bodies}\n" +
                          $"TRIS: {catalogItem.metrics.triangles}";

        string rightText = $"TEXTURES: {catalogItem.metrics.textures}\n" +
                           $"MATERIALS: {catalogItem.metrics.materials}\n" +
                           $"GEOMETRIES: {catalogItem.metrics.meshes}";

        entityInformationView.SeEntityLimitsLeftText(leftText);
        entityInformationView.SeEntityLimitsRightText(rightText);
    }

    public void Enable()
    {
        entityInformationView.SetActive(true);
        entityInformationView.isEnable = true;
    }

    public void Disable()
    {
        entityInformationView.SetActive(false);
        entityInformationView.isEnable = false;
        EntityDeselected();
        entityInformationView.SetCurrentEntity(null);
    }

    internal void EntityDeselected()
    {
        if (entityInformationView.currentEntity == null)
            return;

        if (entityInformationView.currentEntity.rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out BaseComponent component))
        {
            SmartItemComponent smartItemComponent = (SmartItemComponent)component;
            SmartItemComponent.Model modelo = smartItemComponent.model;
            modelo.ToString();
            OnSmartItemComponentUpdate?.Invoke(entityInformationView.currentEntity);
        }
    }

    public void UpdateInfo(DCLBuilderInWorldEntity entity)
    {
        if (entity.gameObject != null)
        {
            Vector3 positionConverted = WorldStateUtils.ConvertUnityToScenePosition(entity.gameObject.transform.position, parcelScene);
            Vector3 currentRotation = entity.gameObject.transform.rotation.eulerAngles;
            Vector3 currentScale = entity.gameObject.transform.localScale;

            var newEuler = currentRotation;

            newEuler.x = RepeatWorking(newEuler.x - currentRotation.x + 180.0F, 360.0F) + currentRotation.x - 180.0F;
            newEuler.y = RepeatWorking(newEuler.y - currentRotation.y + 180.0F, 360.0F) + currentRotation.y - 180.0F;
            newEuler.z = RepeatWorking(newEuler.z - currentRotation.z + 180.0F, 360.0F) + currentRotation.z - 180.0F;

            currentRotation = newEuler;

            entityInformationView.SetPositionAttribute(positionConverted);
            entityInformationView.SetRotationAttribute(currentRotation);
            entityInformationView.SetScaleAttribute(currentScale);
        }
    }

    internal float RepeatWorking(float t, float length)
    {
        return (t - (Mathf.Floor(t / length) * length));
    }
}
