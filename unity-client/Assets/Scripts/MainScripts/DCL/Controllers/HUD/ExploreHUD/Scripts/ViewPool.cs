using System.Collections.Generic;
using UnityEngine;
using System;

internal class ViewPool<T> : IDisposable where T : MonoBehaviour
{
    T baseView;
    public Queue<T> pooledHotScenCells { get; } = new Queue<T>();

    public ViewPool(T baseView, int prewarm = 0)
    {
        this.baseView = baseView;

        PoolView(baseView);
        for (int i = 0; i < prewarm - 1; i++)
        {
            PoolView(CreateView());
        }
    }

    public void Dispose()
    {
        while (pooledHotScenCells.Count > 0)
        {
            var obj = pooledHotScenCells.Dequeue();
            if (obj != null)
            {
                GameObject.Destroy(obj.gameObject);
            }
        }
    }

    public T GetView()
    {
        T ret = pooledHotScenCells.Count > 0 ? pooledHotScenCells.Dequeue() : CreateView();
        ret.gameObject.SetActive(false);
        return ret;
    }

    public void PoolView(T cellView)
    {
        cellView.gameObject.SetActive(false);
        pooledHotScenCells.Enqueue(cellView);
    }

    T CreateView()
    {
        return GameObject.Instantiate(baseView, baseView.transform.parent);
    }
}
