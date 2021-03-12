using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public interface IDelayedComponent : IComponent
    {
        WaitForComponentUpdate yieldInstruction { get; }
        Coroutine routine { get; }
        bool isRoutineRunning { get; }
    }

    public interface IEntityComponent : IComponent
    {
        DecentralandEntity entity { get; set; }
        Transform transform { get; }
    }

    public interface IComponent : ICleanable
    {
        string id { get; set; }
        IParcelScene scene { get; set; }
        string componentName { get; }
        void UpdateFromJSON(string json);
        void UpdateFromModel(BaseModel model);
        IEnumerator ApplyChanges(BaseModel model);
        void RaiseOnAppliedChanges();
        bool IsValid();
        BaseModel GetModel();
        int GetClassId();
        void Initialize();
    }

    /// <summary>
    /// Unity is unable to yield a coroutine while is already being yielded by another one.
    /// To fix that we wrap the routine in a CustomYieldInstruction.
    /// </summary>
    public class WaitForComponentUpdate : CleanableYieldInstruction
    {
        public IDelayedComponent component;

        public WaitForComponentUpdate(IDelayedComponent component)
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

    public abstract class BaseComponent : MonoBehaviour, IEntityComponent, IDelayedComponent, IPoolLifecycleHandler, IPoolableObjectContainer
    {
        protected ComponentUpdateHandler updateHandler;
        public WaitForComponentUpdate yieldInstruction => updateHandler.yieldInstruction;
        public Coroutine routine => updateHandler.routine;
        public bool isRoutineRunning => updateHandler.isRoutineRunning;

        public IParcelScene scene { get; set; }

        public string id { get; set; }

        public DecentralandEntity entity { get; set; }

        public PoolableObject poolableObject { get; set; }

        public string componentName => "BaseComponent";

        protected BaseModel model;

        public void RaiseOnAppliedChanges()
        {
        }

        public virtual void Initialize()
        {
            transform.SetParent(entity.gameObject.transform, false);
        }

        public virtual void UpdateFromJSON(string json)
        {
            UpdateFromModel(model.GetDataFromJSON(json));
        }

        public virtual void UpdateFromModel(BaseModel newModel)
        {
            model = newModel;
            updateHandler.ApplyChangesIfModified(model);
        }

        public abstract IEnumerator ApplyChanges(BaseModel model);

        void OnEnable()
        {
            if (updateHandler == null)
                updateHandler = CreateUpdateHandler();
        }

        public virtual BaseModel GetModel() => model;

        protected virtual ComponentUpdateHandler CreateUpdateHandler()
        {
            return new ComponentUpdateHandler(this);
        }

        public bool IsValid()
        {
            return this != null;
        }

        public virtual void Cleanup()
        {
            updateHandler.Cleanup();
        }

        public virtual void OnPoolRelease()
        {
            Cleanup();
        }

        public virtual void OnPoolGet()
        {
            if (updateHandler == null)
                updateHandler = CreateUpdateHandler();
        }

        public abstract int GetClassId();
    }
}