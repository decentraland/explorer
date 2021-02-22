using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuilderProjectsPanelDataMock
{
    private GameObject bridge;

    private Dictionary<string, BuilderProjectsPanelSceneDataMock> projects =
        new Dictionary<string, BuilderProjectsPanelSceneDataMock>();

    public BuilderProjectsPanelDataMock(GameObject bridge)
    {
        this.bridge = bridge;
        GenerateMockProjects();
    }

    public void SendFetchProjects()
    {
        FakeResponse("OnReceivedProjects",FakeProjectsPayload());
    }

    public void SendDuplicateProject(string id)
    {
        if (!projects.TryGetValue(id, out BuilderProjectsPanelSceneDataMock project))
        {
            return;
        }

        string newId = project.id + project.id.Substring(project.id.Length - 1);
        while(projects.ContainsKey(newId))
            newId += newId.Substring(newId.Length - 1);

        var newProject = project;
        newProject.id = newId;
        newProject.isDeployed = false;
        projects.Add(newId, newProject);

        SendFetchProjects();
    }

    public void SendUnPublish(string id)
    {
        if (!projects.TryGetValue(id, out BuilderProjectsPanelSceneDataMock project))
        {
            return;
        }

        project.isDeployed = false;
        projects[project.id] = project;
        SendFetchProjects();
    }

    public void SendDelete(string id)
    {
        if (projects.Remove(id))
        {
            SendFetchProjects();
        }
    }

    public void SendQuitContributor(string id)
    {
        if (!projects.TryGetValue(id, out BuilderProjectsPanelSceneDataMock project))
        {
            return;
        }

        project.isContributor = false;
        projects[project.id] = project;
        SendFetchProjects();
    }

    private void GenerateMockProjects()
    {
        const int deployedCount = 4;
        const int projectCount = 6;

        for (int i = 0; i < deployedCount; i++)
        {
            int role = Random.Range(0, 3);
            string id = $"MyDeployedScene{i}";
            projects.Add(id, new BuilderProjectsPanelSceneDataMock()
            {
                id = id,
                name = $"MyDeployedScene{i}",
                isDeployed = true,
                isOwner = role == 0,
                isOperator = role == 1,
                isContributor = role == 2,
                size = new Vector2Int(Random.Range(1,6),Random.Range(1,6)),
                coords = new Vector2Int(Random.Range(-100,100),Random.Range(-100,100)),
            });
        }
        for (int i = 0; i < projectCount; i++)
        {
            int role = Random.Range(0, 2);
            string id = $"MyProject{i}";
            projects.Add(id, new BuilderProjectsPanelSceneDataMock()
            {
                id = id,
                name = $"MyProject{i}",
                isDeployed = false,
                isOwner = role == 0,
                isContributor = role == 1,
                size = new Vector2Int(Random.Range(1,6),Random.Range(1,6)),
                coords = new Vector2Int(Random.Range(-100,100),Random.Range(-100,100)),
            });
        }
    }

    private string FakeProjectsPayload()
    {
        string value = "[";
        var projectsArray = projects.Values.ToArray();
        for (int i = 0; i < projectsArray.Length; i++)
        {
            value += JsonUtility.ToJson(projectsArray[i]);
            if (i < projectsArray.Length - 1) value += ",";
        }
        value += "]";
        return value;
    }

    private void FakeResponse(string method, string payload)
    {
        bridge.SendMessage(method,payload);
    }
}
