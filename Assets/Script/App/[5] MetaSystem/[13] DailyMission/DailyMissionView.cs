using IGCore.MVCS;
using UnityEngine;
using App.GamePlay.IdleMiner.PopupDialog;
using TMPro;

public class DailyMissionView : APopupDialog
{
    [SerializeField] TMP_Text txtDesc;

    public class Presentor : APresentor
    {
        public Presentor(string desc)
        { 
            txtDesc = desc;
        }
        public string txtDesc { get; private set; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }


    public override void Refresh(APresentor presentData)
    {
        Presentor presentor = presentData as Presentor;
        if(presentor == null)
            return;

        txtDesc.text = presentor.txtDesc;
    }
    
}
