using UnityEngine;

namespace IGCore.MVCS
{
    public abstract class APlayerModel
    {
        public AContext context { get; protected set; }
        public bool IsInitialized { get; protected set; } = false;

        public APlayerModel(AContext ctx)
        {
            context = ctx;
        }

        public abstract void Init();

        
        public virtual void Dispose() { }
    }
}
