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
    public System.Action<BuildModeEntityListController.EntityAction, DecentralandEntity> OnActioninvoked;
    DecentralandEntity currentEntity;

    public void SetContent(DecentralandEntity _decentrelandEntity)
    {
        currentEntity = _decentrelandEntity;
        nameTxt.text = currentEntity.entityId;
        if (currentEntity.gameObject.activeSelf) showImg.color = selectedColor;
        if (currentEntity.isLocked) lockImg.color = Color.white;
        else lockImg.color = selectedColor;
    }

    public void SelectOrDeselect()
    {
        selectedImg.enabled = !selectedImg.enabled;
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.SELECT,currentEntity);
    }
    public void ShowOrHide()
    {
        if (currentEntity.gameObject.activeSelf) showImg.color = Color.white;
        else showImg.color = selectedColor;
         OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.SHOW, currentEntity);
    }

    public void LockOrUnlock()
    {
        if (currentEntity.isLocked) lockImg.color = Color.white;
        else lockImg.color = selectedColor;
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.LOCK, currentEntity);
    }

    public void DeleteEntity()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.DELETE, currentEntity);
    }

    public void DuplicateEntity()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.DUPLICATE, currentEntity);
    }

    public void GroupEntity()
    {

    }
}
