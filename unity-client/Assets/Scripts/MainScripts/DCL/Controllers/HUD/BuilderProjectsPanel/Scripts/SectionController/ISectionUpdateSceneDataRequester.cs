using System;

internal interface ISectionUpdateSceneDataRequester
{
    event Action<string, SceneUpdatePayload> OnRequestUpdateSceneData;
}