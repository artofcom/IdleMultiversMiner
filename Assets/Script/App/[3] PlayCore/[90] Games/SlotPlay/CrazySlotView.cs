using UnityEngine;

public class CrazySlotView : IGCore.MVCS.AView
{
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

        // txtLoading.text = presentor.loadingMsg;
    }
}
