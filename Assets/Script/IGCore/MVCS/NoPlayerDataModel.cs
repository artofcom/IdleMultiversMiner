using UnityEngine.Assertions;

namespace IGCore.MVCS
{
    public class NoPlayerDataModel : AModel
    {
        public NoPlayerDataModel(AContext context, APlayerModel playerData) : base(context, playerData) 
        {
            Assert.IsTrue(playerData == null, "NoPlayerDataModel should NOT have their playerModel.");
        }


        public override void Init(object data = null)
        { 
            base.Init(data);
        }

        public override void Dispose() {    base.Dispose();     }
    }
}
         