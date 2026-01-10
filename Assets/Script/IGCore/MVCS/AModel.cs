using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace IGCore.MVCS
{
    public abstract class AModel
    {
        protected AContext context;
        protected APlayerModel playerData;

        public AModel(AContext context, APlayerModel playerData)
        {
            this.context = context;
            this.playerData = playerData;
        }


        protected bool _isInitialized = false;
        public bool IsInitialized => _isInitialized && playerData.IsInitialized;
        
        protected bool isDisposed = true;

        public virtual void Init(object data = null)
        {
            Assert.IsTrue(isDisposed, $"Plese dispose the module first before call Init ! : [{this.GetType().Name}]" );
            isDisposed = false;
        }
        
        
        public virtual void Dispose() 
        { 
            isDisposed = true;
        }
    }
}
         