using System;
using UnityEngine;

namespace IGCore.MVCS
{
    public abstract class AController
    {
        protected AUnit unit;
        protected AContext context;
        protected AView view;
        protected AModel model;
        
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

        public abstract void Init();
        public abstract void Resume(int awayTimeInSec);
        public abstract void Pump();
        public abstract void WriteData();
        public virtual void Dispose() 
        {
            if(!context.IsSimulationMode())
            {
                view.OnViewDisable -= OnViewDisable;
                view.OnViewDisable -= OnViewDisable;
                Debug.Log($"[controller] [{GetType().Name}] dispose called.");
            }
        }

        protected abstract void OnViewEnable();
        protected abstract void OnViewDisable();
         
    }
}
