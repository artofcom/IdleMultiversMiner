using UnityEngine;
using IGCore.MVCS;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class PopupDialogView : IGCore.MVCS.AView
    {
        [SerializeField] List<APopupDialog> dialogs;

        public List<APopupDialog> Dialogs => dialogs;

        protected virtual void Awake()
        {
            Assert.IsTrue(dialogs!=null &&  dialogs.Count>0);
        }

        public override void Refresh(APresentor presentInfo) {}
    }
}