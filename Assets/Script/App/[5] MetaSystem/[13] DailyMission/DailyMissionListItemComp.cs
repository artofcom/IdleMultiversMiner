using IGCore.MVCS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;

public class DailyMissionListItemComp : AView
{
    public Action<int> EventOnBtnClaimClicked;

    [SerializeField] GameObject workingRoot;
    [SerializeField] GameObject claimableRoot;
    
    [SerializeField] Image missionIcon;
    [SerializeField] TMP_Text txtTitle;
    [SerializeField] TMP_Text txtDesc;
    
    [SerializeField] Image workingRewardIcon;
    [SerializeField] TMP_Text workingCountStatus;
    [SerializeField] Slider workingCountSlider;
    [SerializeField] TMP_Text workingRewardAmount;

    [SerializeField] Image claimableRewardIcon;
    [SerializeField] TMP_Text claimableRewardAmount;
    [SerializeField] GameObject rewardableRoot;
    [SerializeField] GameObject rewardedRoot;


    int Id;

    public class Presentor : APresentor
    {
        public Presentor() { }

        // Working.
        public Presentor(int id, Sprite icon, string txtTitle, string txtDesc, Sprite rewardIcon, long rewardAmount, long goalCount, long currentCount)
        {
            IsClaimable = false;

            this.Id = id;
            this.icon = icon;
            this.txtTitle = txtTitle;
            this.txtDesc = txtDesc;
            
            this.goalCount = goalCount;
            this.currentCount = currentCount;
            
            this.rewardIcon = rewardIcon;
            this.rewardAmount = rewardAmount;
        }

        // Ready To Claim.
        public Presentor(int id, Sprite icon, string txtTitle, string txtDesc, Sprite rewardIcon, long rewardAmount, bool claimed)
        {
            IsClaimable = true;

            this.Id = id;
            this.icon = icon;
            this.txtTitle = txtTitle;
            this.txtDesc = txtDesc;
            
            this.rewardIcon = rewardIcon;
            this.rewardAmount = rewardAmount;

            this.IsClaimed = claimed;
        }

        public int Id { get;  private set; }
        public bool IsClaimable { get;  private set; }
        public Sprite icon { get; private set; }
        public string txtTitle { get; private set; }
        public string txtDesc { get; private set; }
        public Sprite rewardIcon { get; private set; }
        public long rewardAmount { get; private set; }
        public long goalCount { get; private set; }
        public long currentCount { get; private set; }
        public bool IsClaimed { get;  private set; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        Assert.IsNotNull(workingRoot);
        Assert.IsNotNull(claimableRoot);
        
        Assert.IsNotNull(missionIcon);
        Assert.IsNotNull(txtTitle);
        Assert.IsNotNull(txtDesc);

        Assert.IsNotNull(workingRewardIcon);
        Assert.IsNotNull(workingRewardAmount);
        Assert.IsNotNull(claimableRewardIcon);
        Assert.IsNotNull(claimableRewardAmount);
    }

    public override void Refresh(APresentor presentor)
    {
        var presentInfo = presentor as Presentor;
        if(presentInfo == null)
            return;

        this.Id = presentInfo.Id;
        workingRoot.SetActive(!presentInfo.IsClaimable);
        claimableRoot.SetActive(presentInfo.IsClaimable);

        txtTitle.text = presentInfo.txtTitle;
        txtDesc.text = presentInfo.txtDesc;

        missionIcon.sprite = presentInfo.icon;

        if(presentInfo.IsClaimable)
        {
            claimableRewardIcon.sprite = presentInfo.rewardIcon;
            claimableRewardAmount.text = presentInfo.rewardAmount.ToString();

            rewardableRoot.SetActive(!presentInfo.IsClaimed);
            rewardedRoot.SetActive(presentInfo.IsClaimed);
        }
        else
        {
            workingRewardIcon.sprite = presentInfo.rewardIcon;
            workingRewardAmount.text = presentInfo.rewardAmount.ToString();

            workingCountSlider.minValue = 0;
            workingCountSlider.maxValue = presentInfo.goalCount;
            workingCountSlider.value = presentInfo.currentCount;
            workingCountStatus.text = $"{presentInfo.currentCount} / {presentInfo.goalCount}";
        }
    }

    public void OnBtnClaimClicked()
    {
        EventOnBtnClaimClicked?.Invoke(Id);
    }
}
