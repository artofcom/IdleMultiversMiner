using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillTreeBottomPnlComp : IGCore.MVCS.AView
    {
        [SerializeField] GameObject objUnselected;
        [SerializeField] GameObject objPnlDetail;

        [SerializeField] TMP_Text textName;
        [SerializeField] TMP_Text textDesc;

        [SerializeField] GameObject objBtnLearn;
        [SerializeField] GameObject objUnreachable;
        [SerializeField] GameObject objLearned;
        [SerializeField] GameObject objReqGroup;

        [SerializeField] List<ResourceStatComp> requiredResources;

        public class PresentInfo : APresentor
        {
            public enum STATUS
            {
                UNREACHABLE, LEARNING, LEARNED 
            };

            // Not selected anything.
            public PresentInfo()
            {}

            public PresentInfo(STATUS status, string name, string desc, List<ResourceStatComp.PresentInfo> _info)
            {
                this.status = status;
                this.name = name;
                this.description = desc;

                reqResourcePresentInfos = _info;
            }

            public STATUS status { get; private set; }
            public string name { get; private set; }
            public string description { get; private set; }
            public List<ResourceStatComp.PresentInfo> reqResourcePresentInfos { get; private set; }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            Assert.IsNotNull(textName);
            Assert.IsNotNull(textDesc);

            Assert.IsNotNull(objBtnLearn);
            Assert.IsNotNull(objUnreachable);
            Assert.IsNotNull(objLearned);
            Assert.IsNotNull(objReqGroup);

            Assert.IsTrue(requiredResources!=null && requiredResources.Count>0);
        }
        
        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;
            if (info == null)
                return;

            if(string.IsNullOrEmpty(info.name))
            {
                objUnselected.SetActive(true);
                objPnlDetail.SetActive(false);
                return;
            }

            objUnselected.SetActive(false);
            objPnlDetail.SetActive(true);


            Assert.IsTrue(requiredResources.Count >= info.reqResourcePresentInfos.Count);

            for(int q = 0; q < requiredResources.Count; ++q)
                requiredResources[q].gameObject.SetActive(q<info.reqResourcePresentInfos.Count);

            bool isLearnable = true;
            for(int q = 0; q < info.reqResourcePresentInfos.Count; ++q)
            {
                if(info.reqResourcePresentInfos[q].TargetCount > info.reqResourcePresentInfos[q].CurCount)
                {
                    isLearnable = false;
                    break;
                }
            }

            textName.text = info.name;
            textDesc.text = info.description;

            //objBtnLearn.SetActive(info.status == PresentInfo.STATUS.LEARNING && isLearnable);
            objUnreachable.SetActive(info.status == PresentInfo.STATUS.UNREACHABLE);
            objLearned.SetActive(info.status == PresentInfo.STATUS.LEARNED);
            objReqGroup.SetActive(info.status == PresentInfo.STATUS.LEARNING);// && !isLearnable);
            objBtnLearn.GetComponent<Button>().interactable = isLearnable;


            if(objReqGroup.activeSelf)
            {
                for(int q = 0; q < info.reqResourcePresentInfos.Count; ++q)
                    requiredResources[q].Refresh(info.reqResourcePresentInfos[q]);
            }
        }
    }
}