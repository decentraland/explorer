using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarName : MonoBehaviour
{
    const float NAME_VANISHING_POINT_DISTANCE = 20.0f;

    public CanvasGroup uiContainer;
    public Transform sourceTransform;
    public TextMeshProUGUI nameText;
    public List<RectTransform> layoutGroupRTs;
    public Vector3 offset;
    Canvas canvas;
    Camera mainCamera;
    RectTransform canvasRect;

    Vector2 res;

    public void SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            uiContainer.alpha = 0;
            return;
        }

        if (nameText.text != name)
        {
            nameText.text = name;

            for (int i = 0; i < layoutGroupRTs.Count; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupRTs[i]);
            }

            RefreshTextPosition();
        }
    }

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasRect = (RectTransform)canvas.transform;
        layoutGroupRTs = new List<RectTransform>();

        LayoutGroup[] groups = transform.GetComponentsInChildren<LayoutGroup>();

        for (int i = 0; i < groups.Length; i++)
        {
            LayoutGroup group = groups[i];
            layoutGroupRTs.Add(group.transform as RectTransform);
        }
    }

    void OnEnable()
    {
        // We initialize mainCamera here because the main camera may change while the gameobject is disabled
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (string.IsNullOrEmpty(nameText.text))
            return;

        RefreshTextPosition();

        if (Screen.width != res.x || Screen.height != res.y)
            nameText.SetAllDirty();

        res = new Vector2(Screen.width, Screen.height);
    }

    private void RefreshTextPosition()
    {
        Vector3 screenPoint = mainCamera == null ? Vector3.zero : mainCamera.WorldToViewportPoint(sourceTransform.position + offset);
        uiContainer.alpha = 1.0f + (1.0f - (screenPoint.z / NAME_VANISHING_POINT_DISTANCE));

        if (screenPoint.z > 0)
        {
            if (!uiContainer.gameObject.activeSelf)
            {
                uiContainer.gameObject.SetActive(true);
            }

            float width = canvasRect.rect.width;
            float height = canvasRect.rect.height;
            screenPoint.Scale(new Vector3(width, height, 0));
            ((RectTransform)transform).anchoredPosition = screenPoint;
        }
        else
        {
            if (uiContainer.gameObject.activeSelf)
            {
                uiContainer.gameObject.SetActive(false);
            }
        }
    }
}
