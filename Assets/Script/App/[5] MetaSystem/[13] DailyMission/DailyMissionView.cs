using App.GamePlay.IdleMiner.PopupDialog;
using App.GamePlay.IdleMiner.Resouces;
using Core.Utils;
using IGCore.Components;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class DailyMissionView : APopupDialog
{
    public Action<int> EventOnBtnClaimClicked;
    public Action EventOnBtnResetClicked;

    [SerializeField] TMP_Text txtDesc;

    [SerializeField] GameObject listItemCache;
    [SerializeField] Transform Content;
    [SerializeField] Transform PoolerParent;

    [ImplementsInterface(typeof(INotificator))] 
    [SerializeField] MonoBehaviour dailyMissionNotificator;

    GameObjectPooler ListItemPooler = new GameObjectPooler();
    List<DailyMissionListItemComp> Items = new List<DailyMissionListItemComp>();
    bool IsStarted = false;

    public INotificator DailyMissionNotificator => dailyMissionNotificator as INotificator;

    public class Presentor : APresentor
    {
        public Presentor(string desc, List<DailyMissionListItemComp.Presentor> listItemPresentor)
        { 
            txtDesc = desc;
            this.listItemPresentInfos = listItemPresentor;
        }

        public List<DailyMissionListItemComp.Presentor> listItemPresentInfos { get ; private set; }
        public string txtDesc { get; private set; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(listItemCache);
        Assert.IsNotNull(Content);
        Assert.IsNotNull(PoolerParent);

        if(IsStarted)
            return;

        listItemCache.SetActive(false);
        ListItemPooler.Create(listItemCache, PoolerParent);
        IsStarted = true;
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        if (!IsStarted) Start();    // Start() gets called AFTER OnEnable().
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        for (int q = 0; q < Items.Count; ++q)
            GameObjectPooler.ReleasePoolItem(ListItemPooler, Items[q].gameObject);
        Items.Clear();

        OnClose();
    }

    public override void Refresh(APresentor presentData)
    {
        if(!IsStarted)  Start();

        Presentor presentor = presentData as Presentor;
        if(presentor == null)
            return;

        bool rebuildList = presentor.listItemPresentInfos.Count != Items.Count;
        if(rebuildList)
        {
            for (int q = 0; q < Items.Count; ++q)
            {
                Items[q].gameObject.GetComponent<DailyMissionListItemComp>().EventOnBtnClaimClicked -= OnBtnClaimClicked;
                GameObjectPooler.ReleasePoolItem(ListItemPooler, Items[q].gameObject);
            }
            Items.Clear();
        }

        for (int q = 0; q < presentor.listItemPresentInfos.Count; ++q)
        {
            var obj = rebuildList ? GameObjectPooler.GetPoolItem(ListItemPooler, Content, Vector3.zero) : Items[q].gameObject;
            obj.SetActive(true);
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, .0f);
            var itemComp = obj.GetComponent<DailyMissionListItemComp>();
            Assert.IsNotNull(itemComp);

            //itemComp.Init(info.ItemPresentInfo[q].resourceId);
            itemComp.Refresh(presentor.listItemPresentInfos[q]);
            if (rebuildList)
            {
                itemComp.EventOnBtnClaimClicked += OnBtnClaimClicked;
                Items.Add(itemComp);
            }
        }

        txtDesc.text = presentor.txtDesc;
    }

    void OnBtnClaimClicked(int goalType)
    {
        EventOnBtnClaimClicked?.Invoke(goalType);
    }
    public void OnBtnResetClicked()
    {
        EventOnBtnResetClicked?.Invoke();
    }
}
