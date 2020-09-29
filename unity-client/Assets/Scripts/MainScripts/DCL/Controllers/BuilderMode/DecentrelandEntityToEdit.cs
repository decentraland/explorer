using DCL;
using DCL.Models;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecentrelandEntityToEdit 
{
    public string entityUniqueId;
    public DecentralandEntity entity;

    public bool isLocked = false;
    public bool isSelected = false;
    public bool isNew = false;

    Transform originalParent;



    Material[] originalMaterials;

    Material editMaterial;

    float currentScaleAdded, currentYRotationAdded;
    GameObject entityCollider;

    Dictionary<string, GameObject> collidersDictionary = new Dictionary<string, GameObject>();

    public DecentrelandEntityToEdit(DecentralandEntity _entity, Material _editMaterial)
    {
        entity = _entity;
        entity.OnShapeUpdated += ShapeUpdate;
   
        editMaterial = _editMaterial;

        entityUniqueId = entity.scene.sceneData.id + entity.entityId;

        if (entity.meshRootGameObject && entity.meshesInfo.renderers.Length > 0)
        {
            CreateCollidersForEntity(entity);
        }
    }

    public void Select()
    {
        isSelected = true;
        originalParent = entity.gameObject.transform.parent;
        SetEditMaterials();
        SceneController.i.boundariesChecker.AddPersistent(entity);
    }


    public void Deselect()
    {
        isSelected = false;
        entity.gameObject.transform.SetParent(originalParent);
        SceneController.i.boundariesChecker.RemoveEntityToBeChecked(entity);
        SetOriginalMaterials();
    }

    public void CreateColliders()
    {
        if (entity.meshRootGameObject && entity.meshesInfo.renderers.Length > 0)
        {
            CreateCollidersForEntity(entity);
        }
    }
    public void DestroyColliders()
    {
        foreach (GameObject entityCollider in collidersDictionary.Values)
        {
            GameObject.Destroy(entityCollider);
        }
        collidersDictionary.Clear();
    }

    void SetOriginalMaterials()
    {
        if (entity.meshesInfo.renderers != null)
        {
            //originalRenderers.material = originalMaterials;
            int cont = 0;
            foreach (Renderer renderer in entity.meshesInfo.renderers)
            {
                renderer.material = originalMaterials[cont];
                cont++;
            }
        }
    }
    void SetEditMaterials()
    {
        if (entity.meshesInfo.renderers != null && entity.meshesInfo.renderers.Length >= 1)
        {
            originalMaterials = new Material[entity.meshesInfo.renderers.Length];
            int cont = 0;
            foreach (Renderer renderer in entity.meshesInfo.renderers)
            {
                originalMaterials[cont] = renderer.material;
                renderer.material = editMaterial;
                cont++;
            }
        }
    }

    void ShapeUpdate(DecentralandEntity decentralandEntity)
    {
        if (isSelected) SetEditMaterials();
        CreateCollidersForEntity(decentralandEntity);
    }

    void CreateCollidersForEntity(DecentralandEntity entity)
    {
        DecentralandEntity.MeshesInfo meshInfo = entity.meshesInfo;
        if (meshInfo == null || meshInfo.currentShape == null) return;
        if (!meshInfo.currentShape.IsVisible()) return;
        if (!meshInfo.currentShape.IsVisible() && meshInfo.currentShape.HasCollisions()) return;
        if (!meshInfo.currentShape.IsVisible() && !meshInfo.currentShape.HasCollisions()) return;

        if (!collidersDictionary.ContainsKey(entity.scene.sceneData.id + entity.entityId))
        {
            if (entity.children.Count > 0)
            {
                using (var iterator = entity.children.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        CreateCollidersForEntity(iterator.Current.Value);
                    }
                }
            }


            GameObject entityCollider = new GameObject(entity.entityId);
            entityCollider.layer = LayerMask.NameToLayer("OnBuilderPointerClick");


            for (int i = 0; i < meshInfo.renderers.Length; i++)
            {
                Transform t = entityCollider.transform;
                t.SetParent(meshInfo.renderers[i].transform);
                t.ResetLocalTRS();

                var meshCollider = entityCollider.AddComponent<MeshCollider>();
                //meshCollider.convex = true;
                //meshCollider.isTrigger = true;
                if (meshInfo.renderers[i] is SkinnedMeshRenderer)
                {
                    Mesh meshColliderForSkinnedMesh = new Mesh();
                    (meshInfo.renderers[i] as SkinnedMeshRenderer).BakeMesh(meshColliderForSkinnedMesh);
                    meshCollider.sharedMesh = meshColliderForSkinnedMesh;
                    t.localScale = new Vector3(1 / entity.gameObject.transform.lossyScale.x, 1 / entity.gameObject.transform.lossyScale.y, 1 / entity.gameObject.transform.lossyScale.z);
                }
                else
                {
                    meshCollider.sharedMesh = meshInfo.renderers[i].GetComponent<MeshFilter>().sharedMesh;
                }
                meshCollider.enabled = meshInfo.renderers[i].enabled;

            }

            collidersDictionary.Add(entity.scene.sceneData.id + entity.entityId, entityCollider);
        }
    }
}
