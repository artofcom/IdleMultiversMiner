using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Events;
using Unity.VisualScripting;
using System;

namespace App.GamePlay.IdleMiner
{
    public class BoosterItemComp : ARefreshable
    {
        public static Action<string> EventOnPnlClicked;

        [SerializeField] Image Icon;
        [SerializeField] TMP_Text Name;
        [SerializeField] TMP_Text Desc;
        [SerializeField] TMP_Text PriceOrCount;


        public class PresentInfo : IPresentor
        {
            public PresentInfo(string _boosterId, Sprite _sprIcon, string _name, string _desc, string _price, string _ownedCount)
            {
                BoosterId = _boosterId;
                SpriteIcon = _sprIcon;
                Name = _name;
                Desc = _desc;
                OwnedCount = _ownedCount;
                Price = _price;
            }

            public string BoosterId { get; private set; }
            public Sprite SpriteIcon { get; private set; }
            public string Name { get; private set; }
            public string Desc { get; private set; }
            public string OwnedCount { get; private set; }
            public string Price { get; private set; }
        }

        public string BoosterId { get; private set; } = string.Empty;

        // Start is called before the first frame update
        void Start()
        {
            UnityEngine.Assertions.Assert.IsNotNull(Icon);
            UnityEngine.Assertions.Assert.IsNotNull(Name);
            UnityEngine.Assertions.Assert.IsNotNull(Desc);
            UnityEngine.Assertions.Assert.IsNotNull(PriceOrCount);
        }

        public override void Refresh(IPresentor presentor)
        {
            var info = (PresentInfo)presentor;

            this.BoosterId = info.BoosterId;

            Icon.sprite = info.SpriteIcon;
            Name.text = info.Name;
            Desc.text = info.Desc;
            PriceOrCount.text = string.IsNullOrEmpty(info.OwnedCount) || info.OwnedCount=="0" ? info.Price : info.OwnedCount;
        }

        public void OnPnlClicked()
        {
            EventOnPnlClicked?.Invoke(this.BoosterId);
        }
    }

}
