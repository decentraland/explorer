using UnityEngine;

[System.Serializable]
public class QuestTask
{
    public string id;
    public string type;
    public string payload;
    public string name;
    public string coordinates;
}

[System.Serializable]
public class TaskPayload_Single
{
    public bool isDone;

    public float Progress() => isDone ? 1 : 0;
}

[System.Serializable]
public class TaskPayload_Count
{
    public int start;
    public int end;
    public int current;

    public float Progress() => Mathf.InverseLerp(start, end, current);
}