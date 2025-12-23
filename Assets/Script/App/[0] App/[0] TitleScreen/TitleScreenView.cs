using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleScreenView : IGCore.MVCS.AView  // AView
{
    [SerializeField] TMP_Text txtLoading;

    // Start is called before the first frame update
    void Awake()
    {
        UnityEngine.Assertions.Assert.IsNotNull(txtLoading);
    }

    public void OnClickBtnStart()
    {
        // EventSystem.DispatchEvent("TitleScreenView_OnClickBtnStart");
    }

    public class Presentor : APresentor
    {
        public Presentor(string loadingMsg)
        {
            this.loadingMsg = loadingMsg;
        }   

        public string loadingMsg { get; private set; } 
    }


    public override void Refresh(APresentor presentData)
    {
        Presentor presentor = presentData as Presentor;
        if(presentor == null)
            return;

        txtLoading.text = presentor.loadingMsg;
    }
}
