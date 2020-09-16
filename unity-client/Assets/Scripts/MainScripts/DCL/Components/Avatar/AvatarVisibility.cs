using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AvatarVisibility : MonoBehaviour
    {

        [Tooltip("Game objects that should be hidden/shown")]
        public GameObject[] gameObjectsToToggle;
        // A list of all the callers that want to hide the avatar
        private readonly HashSet<string> callsToHide = new HashSet<string>();

        public void SetVisibility(string callerId, bool visibility)
        {
            if (visibility)
            {
                bool removed = callsToHide.Remove(callerId);
                if (removed && callsToHide.Count == 0)
                {
                    // Show
                    SetVisibilityForGameObjects(true);
                }
            }
            else
            {
                bool added = callsToHide.Add(callerId);
                if (added && callsToHide.Count == 1)
                {
                    // Hide
                    SetVisibilityForGameObjects(false);
                }
            }
        }

        private void SetVisibilityForGameObjects(bool value)
        {
            foreach (GameObject gameObject in gameObjectsToToggle)
            {
                gameObject.SetActive(value);
            }
        }
    }
}