using IGCore.MVCS;
using UnityEngine;
using App.GamePlay.IdleMiner.PopupDialog;

public class ShopView : APopupDialog
{
    public class Presentor : APresentor
    {

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
    }
    
}
