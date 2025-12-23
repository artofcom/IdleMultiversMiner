using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;
using UnityEngine.Assertions;
using Core.Utils;

namespace App.GamePlay.IdleMiner.Craft
{
    public class FrameResourceComp : MonoBehaviour
    {
        [SerializeField] Image Icon;
        [SerializeField] TMP_Text TxtName;
        [SerializeField] TMP_Text TxtCount;
        [SerializeField] TMP_Text TxtClass;
        [SerializeField] GameObject PnlRoot;

        public class PresentInfo
        {
            // Collecting Mode.
            public PresentInfo(Sprite _sprItem, string _name, string _class, int _targetCnt, BigInteger _curCnt)
            {
                Visible = true;
                SpriteItem = _sprItem;
                Name = _name;
                Class = _class;
                TargetCount = _targetCnt;
                CurCount = _curCnt;
            }

            // Info Display Mode.
            public PresentInfo(Sprite _sprItem, string _name, string _class, BigInteger _curCnt)
            {
                Visible = true;
                SpriteItem = _sprItem;
                Class = _class;
                Name = _name;
                CurCount = _curCnt;
                TargetCount = -1;
            }

            // No Data Mode. (Hide Main Root.)
            public PresentInfo()
            {
                Visible = false;
            }

            public Sprite SpriteItem { get; private set; }
            public string Name { get; private set; }
            public string Class { get; private set; }
            public int TargetCount { get; private set; }
            public BigInteger CurCount { get; private set; }
            public bool Visible { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(Icon);
            Assert.IsNotNull(TxtName);
            Assert.IsNotNull(PnlRoot);
            Assert.IsNotNull(TxtClass);
        }

        public void Refresh(PresentInfo presentor)
        {
            if (presentor == null)
                return;

            if(!presentor.Visible)
            {
                PnlRoot.SetActive(false);
                return;
            }

            PnlRoot.SetActive(true);

            Icon.sprite = presentor.SpriteItem;
            TxtName.text = presentor.Name;
            TxtClass.text = presentor.Class;

            // Count setting is optional.
            if (TxtCount != null)
            {
                if (presentor.TargetCount >= 0)
                {
                    TxtCount.text = $"{presentor.CurCount.ToAbbString()}/{presentor.TargetCount}";
                    TxtCount.color = presentor.CurCount < presentor.TargetCount ? Color.red : Color.white;
                }
                else
                {
                    TxtCount.text = presentor.CurCount.ToAbbString();
                    TxtCount.color = Color.white;
                }
            }
        }
    }
}
