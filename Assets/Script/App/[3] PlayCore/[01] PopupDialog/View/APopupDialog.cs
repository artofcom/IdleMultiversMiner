using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public abstract class APopupDialog : IGCore.MVCS.AView
    {
        //public string DialogID { get; set; } = "";

        Action<APopupDialog> OnCloseCallback;



        public virtual void InitDialog(string id)
        {
           // DialogID = id;
        }

        public virtual void Display(APresentor presentor, Action<APopupDialog> onCloseCallback)
        {
            //UnityEngine.Assertions.Assert.IsTrue(!string.IsNullOrEmpty(DialogID));

            gameObject.SetActive(true);

            OnCloseCallback = onCloseCallback;

            Refresh(presentor);
        }



        public virtual void OnClose()
        {
            if (OnCloseCallback != null)
                OnCloseCallback.Invoke(this);

            gameObject.SetActive(false);
        }
    }
}
