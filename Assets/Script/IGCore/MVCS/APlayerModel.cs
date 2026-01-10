using UnityEngine.Assertions;

namespace IGCore.MVCS
{
    public abstract class APlayerModel
    {
        public AContext context { get; protected set; }
        public bool IsInitialized { get; protected set; } = false;

        protected bool isDisposed = true;

        public APlayerModel(AContext ctx)
        {
            context = ctx;
        }

        public virtual void Init()
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
