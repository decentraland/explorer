using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityListAdapter : MonoBehaviour
{
    public Color entitySelectedColor,entityUnselectedColor;
    public Color iconsSelectedColor,iconsUnselectedColor;
    public TextMeshProUGUI nameTxt;
    public Image selectedImg, lockImg,showImg;
    public System.Action<BuildModeEntityListController.EntityAction, DecentralandEntityToEdit, EntityListAdapter> OnActioninvoked;
    DecentralandEntityToEdit currentEntity;


    private void OnDestroy()
    {
        if (currentEntity != null)
        {
            currentEntity.onStatusUpdate -= SetInfo;
            currentEntity.OnDelete -= DeleteAdapter;
        }
    }
    public void SetContent(DecentralandEntityToEdit _decentrelandEntity)
    {
        if(currentEntity != null)
        {
            currentEntity.onStatusUpdate -= SetInfo;
            currentEntity.OnDelete -= DeleteAdapter;
        }
        currentEntity = _decentrelandEntity;
        currentEntity.onStatusUpdate += SetInfo;
        currentEntity.OnDelete += DeleteAdapter;

        SetInfo(_decentrelandEntity);
    }


    public void SelectOrDeselect()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.SELECT,currentEntity, this);
    }
    public void ShowOrHide()
    {
         OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.SHOW, currentEntity, this);
    }

    public void LockOrUnlock()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.LOCK, currentEntity, this);
    }

    public void DeleteEntity()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.DELETE, currentEntity, this);
    }

    void SetInfo(DecentralandEntityToEdit entityToEdit)
    {

        nameTxt.text = entityToEdit.rootEntity.entityId;
        if (entityToEdit.rootEntity.gameObject.activeSelf) showImg.color = iconsSelectedColor;
        else showImg.color = iconsUnselectedColor;

        if (entityToEdit.IsLocked) lockImg.color = iconsSelectedColor; 
        else lockImg.color = iconsUnselectedColor;


        if (entityToEdit.IsSelected)  selectedImg.color = entitySelectedColor;
        else selectedImg.color = entityUnselectedColor;
    }
    void DeleteAdapter(DecentralandEntityToEdit entityToEdit)
    {
        if(entityToEdit.entityUniqueId == currentEntity.entityUniqueId) Destroy(gameObject);
    }
}
