using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderPorjectsPanelLocalTest : MonoBehaviour
{
    private BuilderProjectsPanelController controller;
    private IBuilderProjectsPanelBridge bridge;
    
    void Awake()
    {
        controller = new BuilderProjectsPanelController();
        
        if (BuilderProjectsPanelBridge.i == null)
        {
            bridge = new GameObject("_BuilderProjectsPanelBridge").AddComponent<BuilderProjectsPanelBridge>();
        }
        else
        {
            bridge = BuilderProjectsPanelBridge.i;
        }
    }
    void Start()
    {
        if (EventSystem.current == null)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
        controller.Initialize();
        controller.SetVisibility(true);
    }
}
