using System;

public interface IBuilderProjectsPanelBridge
{
    event Action<string> OnProjectsSet;
    event Action<string> OnLandsSet;

    void OnReceivedProjects(string payload);
    void OnReceivedLands(string payload);
    void SendFetchProjects();
    void SendFetchLands();
    void SendDuplicateProject(string id);
    void SendDownload(string id);
    void SendShare(string id);
    void SendUnPublish(string id);
    void SendDelete(string id);
    void SendQuitContributor(string id);
    void SendSceneDataUpdate(string id, SceneDataUpdatePayload payload);
    void SendSceneContributorsUpdate(string id, SceneContributorsUpdatePayload payload);
    void SendSceneAdminsUpdate(string id, SceneAdminsUpdatePayload payload);
    void SendSceneBannedUsersUpdate(string id, SceneBannedUsersUpdatePayload payload);
}
