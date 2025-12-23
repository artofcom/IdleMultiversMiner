using IGCore.MVCS;
using System.Collections.Generic;
using UnityEngine;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class PopupDialogUnit : AUnit
    {
        [SerializeField] string dialogKey;
        public string DialogKey => dialogKey;

        public override void Init(AContext ctx)
        {
            UnityEngine.Assertions.Assert.IsTrue(!string.IsNullOrEmpty(dialogKey));

            base.Init(ctx);

            controller = new PopupDialogController(dialogKey, view, new PopupDialogModel(context, null), context);
        }


        private void OnDestroy()
        {
            base.Dispose();

            controller.Dispose();
        }
    }
}