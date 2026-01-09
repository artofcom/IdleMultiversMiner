using IGCore.MVCS;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class GameCardComp : AView
{
    [SerializeField] TMP_Text txtTitle;
    [SerializeField] TMP_Text txtAwayTime;
    [SerializeField] TMP_Text txtResetCount;
    [SerializeField] GameObject newTagRoot;


    public string GameKey => gameObject.name;

    public class Presentor : APresentor
    {
        public Presentor()
        {
            IsEnabled = false;
        }
        public Presentor(string title, string awayTime, string resetCount, bool isNewGame)
        {
            IsEnabled = true;

            this.txtTitle = title;
            this.txtAwayTime = awayTime;
            this.txtResetCount = resetCount;
            this.isNewGame = isNewGame;
        }   

        public bool IsEnabled { get; private set; }
        public string txtTitle { get; private set; } 
        public string txtAwayTime { get; private set; } 
        public string txtResetCount { get; private set; } 
        public bool isNewGame { get; private set; } 

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsTrue( false == string.IsNullOrEmpty(GameKey) );
        Assert.IsNotNull(newTagRoot);
        Assert.IsNotNull(txtTitle);
        Assert.IsNotNull(txtAwayTime);
        Assert.IsNotNull(txtResetCount);
    }

    // 
    public override void Refresh(APresentor presentData)
    {
        Presentor presentor = presentData as Presentor;
        if(presentor == null)
        {
            DrawDefault();
            return;
        }

        if(!presentor.IsEnabled)
        {
            gameObject.SetActive(false);
            return;
        }

        txtTitle.text = string.IsNullOrEmpty(presentor.txtTitle) ? txtTitle.text : presentor.txtTitle;
        txtAwayTime.text = presentor.txtAwayTime;
        txtResetCount.text = presentor.txtResetCount;
        newTagRoot.SetActive(presentor.isNewGame);
    }

    void DrawDefault()
    {
        // txtTitle.text = string.IsNullOrEmpty(presentor.txtTitle) ? txtTitle.text : presentor.txtTitle;
        txtAwayTime.text = "N/A";
        txtResetCount.text = "Reset : 0";
        newTagRoot.SetActive(false);
    }
}
