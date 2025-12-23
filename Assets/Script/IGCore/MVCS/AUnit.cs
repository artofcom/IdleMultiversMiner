using UnityEngine;
using System;

namespace IGCore.MVCS
{
    public abstract class AUnit : MonoBehaviour
    {
        [SerializeField] protected AView view;
        
        protected AController controller;
        protected AContext context;
        protected AModel model;

        public AView View                   => view;
        public AController Controller       => controller;
        public Action<string> OnEventClose  
        { 
            get => controller?.OnEventClose; 
            set
            {
                if(controller != null)
                    controller.OnEventClose = value; 
            }
        }

        protected virtual void Awake()
        {
            UnityEngine.Assertions.Assert.IsNotNull(view);
        }
        public virtual void Dispose() 
        { 
            controller?.Dispose();
            model?.Dispose();

            Debug.Log($"<color=blue>[Disposing Module] {name} Unit.</color>");
        }

        public virtual void Init(AContext context)
        {
            this.context = context;
        }

        public virtual void Attach()
        {
            Debug.Log("Attaching Unit " + this.name);
            View?.gameObject.SetActive(true);
        }
        public virtual void Detach()
        {
            Debug.Log("Detaching Unit " + this.name);
            View?.gameObject.SetActive(false);
        }
        public virtual void Resume(int awayTimeInSec)
        {
            controller.Resume(awayTimeInSec);
        }
        public virtual void Pump()
        {
            controller.Pump();
        }
        public virtual void WriteData()
        {
            controller.WriteData();
        }


        protected void OnDestroy()
        {
            Dispose();
        }
    }
    
}
