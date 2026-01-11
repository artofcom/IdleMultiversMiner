using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using System; 

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class LevelDetailInfoDialog : APopupDialog
    {
        
        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text txtGameTitle;
        [SerializeField] TMP_Text txtResetCount;
        [SerializeField] TMP_Text txtEarnedStar;
        [SerializeField] TMP_Text txtSkillCount;
        [SerializeField] TMP_Text txtExpectedPlayTime;
        [SerializeField] TMP_Text txtTotalPlayTime;
        
        public class PresentInfo : APresentor
        {            
            public PresentInfo(string gameTitle, string resetCount, string earnedStar, string skillCnt, string designedPlayTime, string totalPlayertime)
            {
                this.gameTitle = gameTitle;
                this.resetCount = resetCount;
                this.earnedStar = earnedStar;
                this.skillCount = skillCnt;
                this.expectedPlayingTime = designedPlayTime;
                this.playtime = totalPlayertime;   
            }

            public string gameTitle { get; private set; }
            public string resetCount { get; private set; }
            public string earnedStar { get; private set; }
            public string skillCount { get; private set; }
            public string expectedPlayingTime { get; private set; }
            public string playtime { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(txtGameTitle);
            Assert.IsNotNull(txtResetCount);
            Assert.IsNotNull(txtEarnedStar);
            Assert.IsNotNull(txtSkillCount);
            Assert.IsNotNull(txtExpectedPlayTime);
            Assert.IsNotNull(txtTotalPlayTime);
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var presentInfo = (PresentInfo)presentor;
            if (presentInfo == null)
                return;

            txtGameTitle.text = presentInfo.gameTitle;
            txtResetCount.text = presentInfo.resetCount;
            txtEarnedStar.text = presentInfo.earnedStar;
            txtSkillCount.text = presentInfo.skillCount;
            txtExpectedPlayTime.text = presentInfo.expectedPlayingTime;  
            txtTotalPlayTime.text = presentInfo.playtime;
        }

        public void OnCloseBtnClicked()
        {
            OnClose();
        }

    }
}
