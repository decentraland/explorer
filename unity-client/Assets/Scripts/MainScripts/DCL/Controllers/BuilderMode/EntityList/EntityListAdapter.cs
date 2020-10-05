using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityListAdapter : MonoBehaviour
{
    public Color selectedColor;
    public TextMeshProUGUI nameTxt;
    public Image selectedImg, lockImg,showImg;
    public System.Action<BuildModeEntityListController.EntityAction, DecentrelandEntityToEdit, EntityListAdapter> OnActioninvoked;
    DecentrelandEntityToEdit currentEntity;

    public void SetContent(DecentrelandEntityToEdit _decentrelandEntity)
    {
        currentEntity = _decentrelandEntity;
        nameTxt.text = currentEntity.rootEntity.entityId;
        if (currentEntity.rootEntity.gameObject.activeSelf) showImg.color = selectedColor;
        //if (currentEntity.isLocked) lockImg.color = Color.white;
        else lockImg.color = selectedColor;
    }

    public void SelectOrDeselect()
    {
        selectedImg.enabled = !selectedImg.enabled;
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.SELECT,currentEntity, this);
    }
    public void ShowOrHide()
    {
        if (currentEntity.rootEntity.gameObject.activeSelf) showImg.color = Color.white;
        else showImg.color = selectedColor;
         OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.SHOW, currentEntity, this);
    }

    public void LockOrUnlock()
    {
        //if (currentEntity.isLocked) lockImg.color = Color.white;
        //else lockImg.color = selectedColor;
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.LOCK, currentEntity, this);
    }

    public void DeleteEntity()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.DELETE, currentEntity, this);
    }

    public void DuplicateEntity()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.DUPLICATE, currentEntity, this);
    }

    public void GroupEntity()
    {

    }
}
