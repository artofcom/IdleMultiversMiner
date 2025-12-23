using System;
using UnityEngine;

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
        
        public abstract void Init();
        
        
        public virtual void Dispose() { }
    }
}
         