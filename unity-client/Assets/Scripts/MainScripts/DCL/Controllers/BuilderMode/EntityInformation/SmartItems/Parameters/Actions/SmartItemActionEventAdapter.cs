using DCL;
using DCL.Components;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemActionEventAdapter : MonoBehaviour
{
    public TMP_Dropdown entityDropDown;
    public TMP_Dropdown actionDropDown;
    public TMP_Dropdown optionsDropDown;
    public SmartItemListView smartItemListView;

    public System.Action<SmartItemActionEventAdapter> OnActionableRemove;


    private SmartItemActionEvent actionEvent;

    private SmartItemComponent selectedComponent;
    private List<DCLBuilderInWorldEntity> filteredList = new List<DCLBuilderInWorldEntity>();

    private void Start()
    {
        entityDropDown.onValueChanged.AddListener(SelectedEntity);
        actionDropDown.onValueChanged.AddListener(GenerateParametersFromIndex);
        optionsDropDown.onValueChanged.AddListener(OptionSelected);
        optionsDropDown.SetValueWithoutNotify(-1);
    }

    private void OptionSelected(int index)
    {
        switch (index)
        {
            case 0:
                ResetActionable();
                break;
            case 1:
                RemoveActionable();
                break;
        }
    }

    public SmartItemActionEvent GetContent()
    {
        return actionEvent;
    }

    public void RemoveActionable()
    {
        OnActionableRemove?.Invoke(this);
        Destroy(gameObject);
    }

    public void ResetActionable()
    {
        SetContent(actionEvent);
    }

    public void SetContent(SmartItemActionEvent actionEvent)
    {
        this.actionEvent = actionEvent;
        actionEvent.smartItemActionable = new SmartItemActionable();
        filteredList = BuilderInWorldUtils.FilterEntitiesBySmartItemComponentAndActions(actionEvent.entityList);

        GenerateEntityDropdownContent();
        SelectedEntity(0);
    }

    private void SelectedEntity(int number)
    {
        if (!filteredList[number].rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out BaseComponent component))
            return;

        actionEvent.smartItemActionable.entityId = filteredList[number].rootEntity.entityId;
        selectedComponent = (SmartItemComponent) component;
        GenerateActionDropdownContent(selectedComponent.model.actions);

        GenerateParametersFromSelectedOption();   
    }

    private void GenerateParametersFromSelectedOption()
    {
        GenerateParametersFromIndex(actionDropDown.value);
    }

    private void GenerateParametersFromIndex(int index)
    {
        string label = actionDropDown.options[index].text;

        SmartItemAction selectedAction = null;
        foreach (SmartItemAction action in selectedComponent.model.actions)
        {
            if (action.label == label)
            {
                selectedAction = action;
                break;
            }
        }

        actionEvent.smartItemActionable.actionId = selectedAction.id;
        smartItemListView.SetEntityList(actionEvent.entityList);
        smartItemListView.SetSmartItemParameters(selectedAction.parameters, actionEvent.values);
    }

    void GenerateActionDropdownContent(SmartItemAction[] actions)
    {
        actionDropDown.ClearOptions();

        actionDropDown.options = new List<TMP_Dropdown.OptionData>();

        List<string> optionsLabelList = new List<string>();
        int index = 0;
        int indexToUse = 0;

        foreach (SmartItemAction action in actions)
        {
            optionsLabelList.Add(action.label);
            if (!string.IsNullOrEmpty(actionEvent.smartItemActionable.actionId) &&
               action.id == actionEvent.smartItemActionable.actionId)
                indexToUse = index;

            index++;
        }

        actionDropDown.AddOptions(optionsLabelList);
        actionDropDown.SetValueWithoutNotify(indexToUse);
    }

    void GenerateEntityDropdownContent()
    {
        entityDropDown.ClearOptions();

        entityDropDown.options = new List<TMP_Dropdown.OptionData>();

        List<string> optionsLabelList = new List<string>();
        int index = 0;
        int indexToUse = 0;

        foreach (DCLBuilderInWorldEntity entity in filteredList)
        {
            optionsLabelList.Add(entity.GetDescriptiveName());
            if (!string.IsNullOrEmpty(actionEvent.smartItemActionable.entityId) &&
                entity.rootEntity.entityId == actionEvent.smartItemActionable.entityId)
                indexToUse = index;

            index++;
        }

        entityDropDown.AddOptions(optionsLabelList);
        entityDropDown.SetValueWithoutNotify(indexToUse);
    }
}
