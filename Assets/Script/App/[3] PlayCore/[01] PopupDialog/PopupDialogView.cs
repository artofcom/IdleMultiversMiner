using UnityEngine;
using IGCore.MVCS;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class PopupDialogView : AView
    {
        [SerializeField] List<APopupDialog> dialogs;
        [SerializeField] List<AUnit> dialogUnits;

        public List<APopupDialog> Dialogs => dialogs;
        public List<AUnit> DialogUnits => dialogUnits;

        protected virtual void Awake()
        {
            Assert.IsTrue(dialogs!=null &&  dialogs.Count>0);
        }

        public override void Refresh(APresentor presentInfo) {}
    }
}