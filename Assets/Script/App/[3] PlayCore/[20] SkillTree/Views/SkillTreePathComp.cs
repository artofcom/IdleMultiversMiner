using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillTreePathComp : ARefreshable
    {
        [SerializeField] List<SkillBtnComp> skillBtnComps;
 //       [SerializeField] SkillPath.eType pathType;

        public List<SkillBtnComp> SkillBtnComps => skillBtnComps;
      //  public SkillPath.eType PathType => pathType;

        public class PresentInfo : IPresentor
        {
            public PresentInfo(List<SkillBtnComp.PresentInfo> _skillBtnCompPresentInfo)
            {
                this.skillBtnPresentInfo = _skillBtnCompPresentInfo;
            }
            public List<SkillBtnComp.PresentInfo> skillBtnPresentInfo { get; private set; }
        }

        private void Awake()
        {
            Assert.IsTrue(skillBtnComps!=null && skillBtnComps.Count>0);
        }

        public override void Refresh(IPresentor _presentInfo)
        {
            var presentInfo = (PresentInfo)_presentInfo;
            Assert.IsNotNull(presentInfo);
            Assert.IsTrue(presentInfo.skillBtnPresentInfo.Count == skillBtnComps.Count);

            for(int q = 0; q < skillBtnComps.Count; ++q)
            {
                skillBtnComps[q].Refresh( presentInfo.skillBtnPresentInfo[q] );
            }
        }
    }
}
