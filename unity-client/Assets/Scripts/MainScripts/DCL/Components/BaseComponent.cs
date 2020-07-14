using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public interface IComponent : ICleanable
    {
        bool isRoutineRunning { get; }
        Coroutine routine { get; }
        string componentName { get; }
        void UpdateFromJSON(string json);
        IEnumerator ApplyChanges(string newJson);
        void RaiseOnAppliedChanges();
        ComponentUpdateHandler CreateUpdateHandler();
    }

    /// <summary>
    /// Unity is unable to yield a coroutine while is already being yielded by another one.
    /// To fix that we wrap the routine in a CustomYieldInstruction.
    /// </summary>
    public class WaitForComponentUpdate : CleanableYieldInstruction
    {
        public IComponent component;

        public WaitForComponentUpdate(IComponent component)
        {
            this.component = component;
        }

        public override bool keepWaiting
        {
            get { return component.isRoutineRunning; }
        }

        public override void Cleanup()
        {
            component.Cleanup();
        }
    }

    public abstract class BaseComponent : MonoBehaviour, IComponent, IPoolLifecycleHandler
    {
        protected ComponentUpdateHandler updateHandler;
        public WaitForComponentUpdate yieldInstruction => updateHandler.yieldInstruction;
        public Coroutine routine => updateHandler.routine;
        public bool isRoutineRunning => updateHandler.isRoutineRunning;

        [NonSerialized] public ParcelScene scene;
        [NonSerialized] public DecentralandEntity entity;
        [NonSerialized] public PoolableObject poolableObject;

        public string componentName => "BaseComponent";

        private IPoolLifecycleHandler[] poolHandlers;

        public virtual void Start()
        {
            if (poolableObject != null)
            {
                poolableObject.OnRelease += OnPoolReleaseWrapper;
                poolableObject.OnGet += OnPoolGetWrapper;
                poolHandlers = GetComponentsInChildren<IPoolLifecycleHandler>();
            }
        }

        public virtual void OnDestroy()
        {
            if (poolableObject != null)
            {
                poolableObject.OnRelease -= OnPoolReleaseWrapper;
                poolableObject.OnGet -= OnPoolGetWrapper;
            }
        }

        public void OnPoolReleaseWrapper()
        {
            for (var i = 0; i < poolHandlers.Length; i++)
            {
                var h = poolHandlers[i];
                h.OnPoolRelease();
            }
        }

        public void OnPoolGetWrapper()
        {
            for (var i = 0; i < poolHandlers.Length; i++)
            {
                var h = poolHandlers[i];
                h.OnPoolGet();
            }
        }

        public virtual void OnPoolRelease()
        {
        }

        public virtual void OnPoolGet()
        {
        }

        public void RaiseOnAppliedChanges()
        {
        }

        public void UpdateFromJSON(string json)
        {
            updateHandler.ApplyChangesIfModified(json);
        }

        void OnEnable()
        {
            if (updateHandler == null)
                updateHandler = CreateUpdateHandler();

            updateHandler.ApplyChangesIfModified(updateHandler.oldSerialization ?? "{}");
        }

        void OnDisable()
        {
            updateHandler.Stop();
        }

        public abstract IEnumerator ApplyChanges(string newJson);

        public virtual ComponentUpdateHandler CreateUpdateHandler()
        {
            return new ComponentUpdateHandler(this);
        }

        public virtual void Cleanup()
        {
            updateHandler.Cleanup();
        }
    }
}