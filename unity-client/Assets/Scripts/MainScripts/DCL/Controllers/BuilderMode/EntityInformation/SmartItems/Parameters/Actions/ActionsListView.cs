using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsListView : ListView<ActionEvent>
{
    public ActionEventAdapter adapter;


    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (ActionEvent actionEvent in contentList)
        {
            ActionEventAdapter adapter = Instantiate(this.adapter, contentPanelTransform).GetComponent<ActionEventAdapter>();
            adapter.SetContent(actionEvent);
        }
    }

    public override void RemoveAdapters()
    {
        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            ActionEventAdapter toRemove = contentPanelTransform.transform.GetChild(i).gameObject.GetComponent<ActionEventAdapter>();
            Destroy(toRemove.gameObject);
        }
    }

    public void AddActionEventAdapter(List<DCLBuilderInWorldEntity> entityList)
    {
        ActionEvent actionEvent = new ActionEvent();
        actionEvent.entityList = entityList;

        contentList.Add(actionEvent);
        RefreshDisplay();
    }
}
