using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class ResourceStatComp : MonoBehaviour
    {
        [SerializeField] Image Icon;
        [SerializeField] TMP_Text TxtName;
        [SerializeField] TMP_Text TxtCount;
        [SerializeField] TMP_Text TxtClass;
        [SerializeField] GameObject PnlRoot;
        [SerializeField] Slider progressSlider;

        public class PresentInfo
        {
            // Collecting Mode.
            public PresentInfo(Sprite _sprItem, string _name, string _class, ulong _targetCnt, BigInteger _curCnt)
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
                Name = _name;
                Class = _class;
                CurCount = _curCnt;
                TargetCount = 0;
            }

            // Target Planet Mode.
            public PresentInfo(Sprite _sprItem, string _name, bool _isCleared)
            {
                Visible = true;
                SpriteItem = _sprItem;
                Name = _name;
                IsCleared = _isCleared;
                PlanetMode = true;
            }

            // No Data Mode. (Hide Main Root.)
            public PresentInfo()
            { 
                Visible = false;
            }

            public Sprite SpriteItem { get; private set; }
            public string Name { get; private set; }
            public string Class { get; private set; }
            public ulong TargetCount { get; private set; } = 0;
            public BigInteger CurCount { get; private set; }
            public bool Visible { get; private set; }
            public bool PlanetMode { get; private set; } = false;
            public bool IsCleared { get; private set; } = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(Icon);
            Assert.IsNotNull(TxtName);
            Assert.IsNotNull(TxtClass);
        }

        public void Refresh(PresentInfo presentor)
        {
            if (presentor == null)
                return;

            if(!presentor.Visible)
            {
                if (PnlRoot != null) PnlRoot.SetActive(false);
                else                 gameObject.SetActive(false);
                return;
            }

            if (PnlRoot != null)    PnlRoot.SetActive(true);
            else                    gameObject.SetActive(true);

            Icon.sprite = presentor.SpriteItem;
            TxtName.text = presentor.Name;
            TxtClass.text = presentor.Class;

            // Count setting is optional.
            if (TxtCount != null)
            {
                if (presentor.PlanetMode)
                {
                    TxtCount.text = presentor.IsCleared ? "Cleared!" : "Boss Battle";
                    TxtCount.color = presentor.IsCleared ? Color.white : Color.red;
                }
                else
                {
                    if (presentor.TargetCount >= 0)
                    {
                        TxtCount.text = $"{presentor.CurCount}/{presentor.TargetCount}";
                        TxtCount.color = presentor.CurCount < presentor.TargetCount ? Color.red : Color.white;


                        if(progressSlider != null)
                        {
                            float progress = ((float)presentor.CurCount) / ((float)presentor.TargetCount);
                            progressSlider.value = progress;
                        }
                    }
                    else
                    {
                        TxtCount.text = $"{presentor.CurCount}";
                        TxtCount.color = Color.white;
                    }
                }
            }
        }
    }
}
