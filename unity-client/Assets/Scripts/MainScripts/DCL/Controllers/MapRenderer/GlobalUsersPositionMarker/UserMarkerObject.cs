using UnityEngine;
using UnityEngine.UI;

public class UserMarkerObject : MonoBehaviour
{
    [SerializeField] Image colorImage = null;

    public Color color
    {
        set
        {
            if (colorImage)
            {
                colorImage.color = value;
            }
        }
    }
}
