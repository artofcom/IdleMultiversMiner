using System;
using UnityEngine.Assertions;
using UnityEngine;

namespace IGCore.MVCS
{
    public abstract class AController
    {
        protected AUnit unit;
        protected AContext context;
        protected AView view;
        protected AModel model;
        
        protected bool isDisposed = true;

        public virtual AContext Context => context;

        public AController(AUnit unit, AView view, AModel model, AContext context)
        {
            this.unit = unit;
            this.view = view;
            this.model = model;
            this.context = context;

            if(!context.IsSimulationMode()) 
            {
                if(view.gameObject.activeSelf && view.gameObject.activeInHierarchy) 
                    OnViewEnable();
            
                view.OnViewEnable += OnViewEnable;
                view.OnViewDisable += OnViewDisable;
                Debug.Log($"[controller] [{GetType().Name}] contrctor called.");
            }
        }

        public virtual void Init()
        {
            Assert.IsTrue(isDisposed, $"Plese dispose the module first before call Init ! : [{this.GetType().Name}]" );
            isDisposed = false;
        }
        public abstract void Resume(int awayTimeInSec);
        public abstract void Pump();
        public abstract void WriteData();
        public virtual void Dispose() 
        {
            if(!context.IsSimulationMode())
            {
                view.OnViewEnable -= OnViewEnable;
                view.OnViewDisable -= OnViewDisable;
                Debug.Log($"[controller] [{GetType().Name}] dispose called.");
            }
            isDisposed = true;
        }

        protected abstract void OnViewEnable();
        protected abstract void OnViewDisable();
         
    }
}
