using IGCore.MVCS;
using UnityEngine;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillTreeUnit : AUnit
    {
        protected APlayerModel playerModel;

        public override void Dispose()
        {
            base.Dispose();
            playerModel?.Dispose();
        }
    }
}