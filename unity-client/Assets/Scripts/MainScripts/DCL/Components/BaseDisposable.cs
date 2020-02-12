using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseDisposable : IComponent
    {
        public virtual string componentName => GetType().Name;
        public string id;

        protected Action<BaseDisposable> OnReadyCallbacks;

        protected ComponentUpdateHandler updateHandler;
        public WaitForComponentUpdate yieldInstruction => updateHandler.yieldInstruction;
        public Coroutine routine => updateHandler.routine;
        public bool isRoutineRunning => updateHandler.isRoutineRunning;

        public event System.Action<DecentralandEntity> OnAttach;
        public event System.Action<DecentralandEntity> OnDetach;
        public event Action<BaseDisposable> OnAppliedChanges;

        public DCL.Controllers.ParcelScene scene { get; }
        public HashSet<DecentralandEntity> attachedEntities = new HashSet<DecentralandEntity>();


        public void UpdateFromJSON(string json)
        {
            updateHandler.ApplyChangesIfModified(json);
        }

        public BaseDisposable(DCL.Controllers.ParcelScene scene)
        {
            this.scene = scene;
            updateHandler = CreateUpdateHandler();
        }

        public virtual void RaiseOnAppliedChanges()
        {
            OnAppliedChanges?.Invoke(this);

            OnReadyCallbacks?.Invoke(this);
            OnReadyCallbacks = null;
        }


        public virtual void AttachTo(DecentralandEntity entity, Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
            {
                return;
            }

            Type thisType = overridenAttachedType != null ? overridenAttachedType : GetType();
            entity.AddSharedComponent(thisType, this);

            attachedEntities.Add(entity);

            entity.OnRemoved += OnEntityRemoved;

            OnAttach?.Invoke(entity);
        }

        private void OnEntityRemoved(DecentralandEntity entity)
        {
            DetachFrom(entity);
        }

        public virtual void DetachFrom(DecentralandEntity entity, Type overridenAttachedType = null)
        {
            if (!attachedEntities.Contains(entity)) return;

            entity.OnRemoved -= OnEntityRemoved;

            Type thisType = overridenAttachedType != null ? overridenAttachedType : GetType();
            entity.RemoveSharedComponent(thisType, false);

            attachedEntities.Remove(entity);

            OnDetach?.Invoke(entity);
        }

        public void DetachFromEveryEntity()
        {
            DecentralandEntity[] attachedEntitiesArray = new DecentralandEntity[attachedEntities.Count];

            attachedEntities.CopyTo(attachedEntitiesArray);

            for (int i = 0; i < attachedEntitiesArray.Length; i++)
            {
                DetachFrom(attachedEntitiesArray[i]);
            }
        }

        public virtual void Dispose()
        {
            DetachFromEveryEntity();
            Resources.UnloadUnusedAssets(); //NOTE(Brian): This will ensure assets are freed correctly.
        }

        public abstract IEnumerator ApplyChanges(string newJson);

        public MonoBehaviour GetCoroutineOwner()
        {
            return scene;
        }

        public virtual ComponentUpdateHandler CreateUpdateHandler()
        {
            return new ComponentUpdateHandler(this);
        }

        public void Cleanup()
        {
            if (isRoutineRunning)
            {
                GetCoroutineOwner().StopCoroutine(routine);
            }
        }

        public virtual void CallWhenReady(Action<IComponent> callback)
        {
            bool applyChangesIsRunning = updateHandler.isRoutineRunning;

            if (!applyChangesIsRunning)
            {
                callback.Invoke(this);
            }
            else
            {
                OnReadyCallbacks += callback;
            }
        }
    }
}
