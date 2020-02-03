using UnityEngine;

public class RenderTimeCounter : MonoBehaviour
{
    public FloatVariable renderTimeVariable;
    float auxRenderTime;

    public void OnPreRender()
    {
        auxRenderTime = Time.realtimeSinceStartup;
    }

    public void OnPostRender()
    {
        renderTimeVariable.Set(Time.realtimeSinceStartup - auxRenderTime);
    }
}
