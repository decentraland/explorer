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
    private const string LEFT_TEXT_FORMAT = "ENTITIES: {0}\nBODIES:{1}\nTIRS:{2}";
    private const string RIGHT_TEXT_FORMAT = "TEXTURES: {0}\nMATERIALS:{1}\nGEOMETRIES:{2}";

    public event Action<Vector3> OnPositionChange;
    public event Action<Vector3> OnRotationChange;
    public event Action<Vector3> OnScaleChange;
    public event Action<DCLBuilderInWorldEntity, string> OnNameChange;
    public event Action<DCLBuilderInWorldEntity> OnSmartItemComponentUpdate;

    internal IEntityInformationView entityInformationView;
    internal ParcelScene parcelScene;
    internal AssetPromise_Texture loadedThumbnailPromise;
    internal bool isChangingName = false;

    public void Initialize(IEntityInformationView entityInformationView)
    {
        this.entityInformationView = entityInformationView;

        if (entityInformationView.position != null)
            entityInformationView.position.OnChanged += PositionChanged;

        if (entityInformationView.rotation != null)
            entityInformationView.rotation.OnChanged += RotationChanged;

        if (entityInformationView.scale != null)
            entityInformationView.scale.OnChanged += ScaleChanged;

        entityInformationView.OnNameChange += NameChanged;
        entityInformationView.OnStartChangingName += StartChangingName;
        entityInformationView.OnEndChangingName += EndChangingName;
        entityInformationView.OnDisable += Disable;
        entityInformationView.OnUpdateInfo += UpdateInfo;
    }

    public void Dispose()
    {
        if (entityInformationView.position != null)
            entityInformationView.position.OnChanged -= PositionChanged;

        if (entityInformationView.rotation != null)
            entityInformationView.rotation.OnChanged -= RotationChanged;

        if (entityInformationView.scale != null)
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
        entityInformationView.SetCurrentEntity(entity);

        if (entityInformationView.currentEntity != null)
        {
            entity.onStatusUpdate -= UpdateEntityName;
            entityInformationView.currentEntity.onStatusUpdate += UpdateEntityName;
        }

        parcelScene = currentScene;

        if (entity.HasSmartItemComponent())
        {
            if (entity.rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out IEntityComponent baseComponent))
                entityInformationView.smartItemList.SetSmartItemParameters(entity.GetSmartItemParameters(), ((SmartItemComponent) baseComponent).GetValues());
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
        if (catalogItem == null)
            return;

        var url = catalogItem.thumbnailURL;

        if (string.IsNullOrEmpty(url))
            return;

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
        if (entity == null)
            return;

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

        string leftText = string.Format(LEFT_TEXT_FORMAT, catalogItem.metrics.entities, catalogItem.metrics.bodies, catalogItem.metrics.triangles);
        string rightText = string.Format(RIGHT_TEXT_FORMAT, catalogItem.metrics.textures, catalogItem.metrics.materials, catalogItem.metrics.meshes);

        entityInformationView.SeEntityLimitsLeftText(leftText);
        entityInformationView.SeEntityLimitsRightText(rightText);
    }

    public void Enable()
    {
        entityInformationView.SetActive(true);
    }

    public void Disable()
    {
        entityInformationView.SetActive(false);
        EntityDeselected();
        entityInformationView.SetCurrentEntity(null);
    }

    internal void EntityDeselected()
    {
        if (entityInformationView.currentEntity == null)
            return;

        if (entityInformationView.currentEntity.rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out IEntityComponent component))
        {
            SmartItemComponent smartItemComponent = (SmartItemComponent) component;
            OnSmartItemComponentUpdate?.Invoke(entityInformationView.currentEntity);
        }
    }

    public void UpdateInfo(DCLBuilderInWorldEntity entity)
    {
        if (entity != null && entity.gameObject != null)
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