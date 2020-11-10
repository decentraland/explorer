using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListView<T> : MonoBehaviour
{

    public Transform contentPanelTransform;

    protected List<T> contentList;
    public void SetContent(List<T> content)
    {
        contentList = content;
        RefreshDisplay();
    }

    public virtual void RefreshDisplay()
    {
        RemoveAdapters();
        AddAdapters();
    }

    public virtual void AddAdapters()
    {

    }

    public virtual void RemoveAdapters()
    {

        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            GameObject toRemove = contentPanelTransform.transform.GetChild(i).gameObject;
            Destroy(toRemove);
        }
    }
}
