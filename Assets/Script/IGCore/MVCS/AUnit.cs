using UnityEngine;
using System;
using UnityEngine.Assertions;

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
        
        public Action<object> OnEventAttached;
        public Action<object> OnEventDetached;

        protected bool isDisposed = true;
        public bool IsAttached => (View==null ? false : View.gameObject.activeSelf && View.gameObject.activeInHierarchy);

        protected virtual void Awake()
        {
            Assert.IsNotNull(view);
        }
        public virtual void Dispose() 
        { 
            controller?.Dispose();
            model?.Dispose();

            isDisposed = true;
            Debug.Log($"<color=blue>[Disposing Module] {name} Unit.</color>");
        }

        public virtual void Init(AContext context)
        {
            this.context = context;
            Assert.IsTrue(isDisposed, $"Plese dispose the module first before call Init ! : [{this.GetType().Name}]" );
            isDisposed = false;
        }

        public virtual void Attach()
        {
            Debug.Log("Attaching Unit " + this.name);
            View?.gameObject.SetActive(true);
            OnEventAttached?.Invoke(this);
        }
        public virtual void Detach()
        {
            Debug.Log("Detaching Unit " + this.name);
            View?.gameObject.SetActive(false);
            OnEventDetached?.Invoke(this);
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


        protected virtual void OnDestroy()
        {
            Dispose();
        }
    }
    
}
