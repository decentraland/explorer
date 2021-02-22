using System;
using UnityEngine;

public class BuilderProjectsPanelBridge : MonoBehaviour
{
    public event Action<string> OnProjectsSet;
    public static BuilderProjectsPanelBridge i { get; private set; }
    public static readonly bool mockData = true;

    private BuilderProjectsPanelDataMock dataMocker = null;

    private void Awake()
    {
        if (i != null)
        {
            Destroy(gameObject);
            return;
        }

        i = this;

        if (mockData)
        {
            dataMocker = new BuilderProjectsPanelDataMock(gameObject);
        }
    }

    public void OnReceivedProjects(string payload)
    {
        OnProjectsSet?.Invoke(payload);
    }

    public void SendFetchProjects()
    {
        dataMocker?.SendFetchProjects();
    }

    public void SendDuplicateProject(string id)
    {
        dataMocker?.SendDuplicateProject(id);
    }

    public void SendDownload(string id)
    {
    }

    public void SendShare(string id)
    {
    }

    public void SendUnPublish(string id)
    {
        dataMocker?.SendUnPublish(id);
    }

    public void SendDelete(string id)
    {
        dataMocker?.SendDelete(id);
    }

    public void SendQuitContributor(string id)
    {
        dataMocker?.SendQuitContributor(id);
    }
}