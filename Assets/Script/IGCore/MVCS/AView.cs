using System;
using System.Collections;
using UnityEngine;

namespace IGCore.MVCS
{
    public abstract class AView : MonoBehaviour
    {
        public abstract class APresentor    { }
        public abstract class AIniter       { }

        public Action OnViewEnable   { get; set; }
        public Action OnViewDisable    { get; set;}

        public virtual void Init(AIniter initer) { }
        public abstract void Refresh(APresentor presentor);

        protected virtual void OnEnable()
        {
            OnViewEnable?.Invoke();
        }
        protected virtual void OnDisable()
        {
            OnViewDisable?.Invoke();
        }
    }
}
