using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class AvatarVisibility : MonoBehaviour
    {

        public AvatarRenderer renderer;
        [Tooltip("Game objects that should be hidden/shown together with the renderer")]
        public GameObject[] gameObjectsToToggle;
        // When this property is true, then the visibility can't be modified
        private Boolean locked = false;
        private Boolean? isVisible;

        public void SetVisibility(Boolean visibility)
        {
            if (locked == false && isVisible != visibility)
            {
                if (visibility)
                {
                    renderer.ShowAllRenderers();
                }
                else
                {
                    renderer.HideAllRenderers();
                }
                foreach (GameObject gameObject in gameObjectsToToggle)
                {
                    gameObject.SetActive(visibility);
                }

                isVisible = visibility;
            }

        }

        public void SetLock(Boolean newLockValue)
        {
            locked = newLockValue;
        }

    }
}