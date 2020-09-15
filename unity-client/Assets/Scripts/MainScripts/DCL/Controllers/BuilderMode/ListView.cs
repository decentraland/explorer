using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListView<T> : MonoBehaviour
{

    public Transform contentPanelTransform;

    protected List<T> contentList;
    public void SetContent(List<T> _content)
    {
        contentList = _content;
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        RemoveAdapters();
        AddAdapters();
    }

    public virtual void AddAdapters()
    {

    }

    public void RemoveAdapters()
    {

        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            GameObject toRemove = contentPanelTransform.transform.GetChild(i).gameObject;
            Destroy(toRemove);
        }
    }
}
