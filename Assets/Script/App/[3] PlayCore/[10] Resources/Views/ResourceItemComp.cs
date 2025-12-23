using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Events;

namespace App.GamePlay.IdleMiner.Resouces
{
    public class ResourceItemComp : ARefreshable
    {
        public const string EVENT_PANEL_CLICKED = "ElementItemComp_OnPnlClicked";
        public const string EVENT_AUTOSELL_CLICKED = "ElementItemComp_OnAutoSellClicked";

        [SerializeField] Image Icon;
        [SerializeField] TMP_Text Name;
        [SerializeField] GameObject AutoSellOn;
        [SerializeField] TMP_Text Count;
        [SerializeField] TMP_Text Price;
        [SerializeField] Image BG;
        [SerializeField] TMP_Text ClassText;
        // [SerializeField] GameObject SelectedImg;

        [Header("Resource Group")]
        [SerializeField] Sprite ItemBG;
        [SerializeField] Sprite ItemSelectedBG;


        public class PresentInfo : IPresentor
        {
            public PresentInfo(string _resourceId, Sprite _sprIcon, string _class,  string _name, string _count, string _price, bool _autoSell, bool _isSelected)
            {
                resourceId = _resourceId;
                spriteIcon = _sprIcon;
                name = _name;
                className = _class;
                count = _count;
                price = _price;
                autoSell = _autoSell;
                IsSelected = _isSelected;
            }

            public string resourceId { get; private set; }
            public Sprite spriteIcon { get; private set; }
            public string name { get; private set; }
            public string className { get; private set; }
            public string count { get; private set; }
            public string price { get; private set; }
            public bool autoSell { get; private set; }
            public bool IsSelected { get; private set; }
        }

        string ResourceId { get; set; } = string.Empty;

        // Start is called before the first frame update
        void Start()
        {
            UnityEngine.Assertions.Assert.IsNotNull(Icon);
            UnityEngine.Assertions.Assert.IsNotNull(Name);
            UnityEngine.Assertions.Assert.IsNotNull(AutoSellOn);
            UnityEngine.Assertions.Assert.IsNotNull(Count);
            UnityEngine.Assertions.Assert.IsNotNull(Price);
            UnityEngine.Assertions.Assert.IsNotNull(BG);
            UnityEngine.Assertions.Assert.IsNotNull(ItemBG);
            UnityEngine.Assertions.Assert.IsNotNull(ItemSelectedBG);
            UnityEngine.Assertions.Assert.IsNotNull(ClassText);
        }

        public void Init(string rscId)
        {
            ResourceId = rscId;    
        }

        public override void Refresh(IPresentor presentor)
        {
            var info = (PresentInfo)presentor;
            Icon.sprite = info.spriteIcon;
            Name.text = info.name;
            AutoSellOn.SetActive(info.autoSell);
            Count.text = info.count;
            Price.text = info.price;
            ClassText.text = info.className;

            BG.sprite = info.IsSelected ? ItemSelectedBG : ItemBG;
        }

        public void OnPnlClicked()
        {
            EventSystem.DispatchEvent(EVENT_PANEL_CLICKED, ResourceId);
        }

        public void OnAutoSellClicked()
        {
            EventSystem.DispatchEvent(EVENT_AUTOSELL_CLICKED, ResourceId);
        }
    }

}
