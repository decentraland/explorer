﻿using UnityEngine;
using DCL.Controllers;
using DCL.Components;

namespace Builder
{
    public class DCLBuilderRaycast : MonoBehaviour
    {
        public const string LAYER_GIZMOS = "Gizmo";
        public const string LAYER_SELECTION = "Selection";

        public Camera builderCamera;

        private const float RAYCAST_MAX_DISTANCE = 10000f;

        public LayerMask defaultMask { get; private set; }
        public LayerMask gizmoMask { get; private set; }

        private Plane groundPlane;
        private Plane entityHitPlane;
        private Plane gizmosHitPlane;

        private void Awake()
        {
            defaultMask = LayerMask.GetMask(DCLBuilderSelectionCollider.LAYER_BUILDER_POINTER_CLICK) | LayerMask.GetMask(LAYER_SELECTION);
            gizmoMask = LayerMask.GetMask(LAYER_GIZMOS);

            groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        public void SetEntityHitPlane(float height)
        {
            entityHitPlane = new Plane(Vector3.up, new Vector3(0, height, 0));
        }

        public void SetGizmoHitPlane(Gizmos.DCLBuilderGizmoAxis gizmoAxis)
        {
            gizmosHitPlane = new Plane(gizmoAxis.GetGizmo().GetPlaneNormal(), gizmoAxis.transform.position);
        }

        public bool Raycast(Vector3 mousePosition, LayerMask mask, out RaycastHit hitInfo, bool checkGizmo = false)
        {
            if (checkGizmo)
            {
                if (Raycast(mousePosition, gizmoMask, out hitInfo))
                {
                    return true;
                }
            }

            return Physics.Raycast(GetMouseRay(mousePosition), out hitInfo, RAYCAST_MAX_DISTANCE, mask);
        }

        public Ray GetMouseRay(Vector3 mousePosition)
        {
            return builderCamera.ScreenPointToRay(mousePosition);
        }

        public bool RaycastToGizmos(Ray ray, out RaycastHit hitInfo)
        {
            return Physics.Raycast(ray, out hitInfo, RAYCAST_MAX_DISTANCE, gizmoMask);
        }

        public bool RaycastToGizmos(Vector3 mousePosition, out RaycastHit hitInfo)
        {
            return RaycastToGizmos(GetMouseRay(mousePosition), out hitInfo);
        }

        public bool RaycastToGround(Vector3 mousePosition, out Vector3 hitPosition)
        {
            Ray ray = GetMouseRay(mousePosition);
            float enter = 0.0f;

            if (groundPlane.Raycast(ray, out enter))
            {
                hitPosition = ray.GetPoint(enter);
                return true;
            }
            hitPosition = Vector3.zero;
            return false;
        }

        public Vector3 RaycastToEntityHitPlane(Vector3 mousePosition)
        {
            Ray ray = GetMouseRay(mousePosition);
            float enter = 0.0f;

            if (entityHitPlane.Raycast(ray, out enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }

        public bool RaycastToGizmosHitPlane(Vector3 mousePosition, out Vector3 hitPosition)
        {
            Ray ray = GetMouseRay(mousePosition);
            float enter = 0.0f;

            if (gizmosHitPlane.Raycast(ray, out enter))
            {
                hitPosition = ray.GetPoint(enter);
                return true;
            }
            hitPosition = Vector3.zero;
            return false;
        }
    }
}