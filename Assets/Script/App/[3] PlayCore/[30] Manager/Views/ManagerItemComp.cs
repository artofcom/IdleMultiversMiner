using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using UnityEngine.UI;

namespace App.GamePlay.IdleMiner
{
    public class ManagerItemComp : MonoBehaviour
    {
        enum STATE {  NODATA = 0, EMPTY, ASSIGNED, MAX };

        [SerializeField] List<GameObject> StateRoots;
        [SerializeField] TMP_Text TxtName;
        //[SerializeField] TMP_Text TxtLevel;
        [SerializeField] TMP_Text TxtRates;
        [SerializeField] Image ImageManager;
        [SerializeField] Image ImagePlanet;
        [SerializeField] Image ImageSelected;
        [SerializeField] StarGradeComp starGradeComp;

        // Event.
        public static System.Action<string> EventOnBtnCardClicked = null;

        public string ManagerInfoId { get; private set; }

        public class PresentInfo
        {
            // Empty Slot.
            public PresentInfo()
            {}

            // Assigned Slot.
            public PresentInfo( string _ownedMngId, string _name, int _level, float _miningRate, float _speedRate, float _packageRate,
                                Sprite sprManager, Sprite sprPlanet, bool _selected)
            {
                OwnedManagerId = _ownedMngId;
                Name = _name;
                Level = _level;
                MiningRate = _miningRate;
                SpeedRate = _speedRate;
                PackageRate = _packageRate;
                ImgManager = sprManager;
                ImgPlanet = sprPlanet;
                IsSelcted = _selected;
            }

            public string OwnedManagerId { get; private set; }
            public string Name { get; private set; }
            public Sprite ImgManager { get; private set; }
            public Sprite ImgPlanet { get; private set; }
            public int Level { get; private set; }
            public float MiningRate { get; private set; }
            public float SpeedRate { get; private set; }
            public float PackageRate { get; private set; }
            public bool IsSelcted { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsTrue(StateRoots != null && StateRoots.Count == (int)STATE.MAX);
            Assert.IsNotNull(TxtName);
            //Assert.IsNotNull(TxtLevel);
            Assert.IsNotNull(TxtRates);
            Assert.IsNotNull(ImageManager);
            Assert.IsNotNull(ImagePlanet);
            Assert.IsNotNull(starGradeComp);
            // Assert.IsNotNull(ImageSelected);
        }

        public void Refresh(PresentInfo presentor)
        {
            for (int q = 0; q < (int)STATE.MAX; ++q)
                StateRoots[q].SetActive(false);

            if (presentor == null)
            {
                StateRoots[(int)STATE.NODATA].SetActive(true);
                return;
            }

            if(string.IsNullOrEmpty(presentor.Name))
            {
                StateRoots[(int)STATE.EMPTY].SetActive(true);
                return;
            }


            ManagerInfoId = presentor.OwnedManagerId;
            Debug.Log("Manager Id : " + ManagerInfoId);

            StateRoots[(int)STATE.ASSIGNED].SetActive(true);

            ImageSelected?.gameObject.SetActive(presentor.IsSelcted);

            TxtName.text = presentor.Name;
            // TxtLevel.text = $"{presentor.Level}/5";
            TxtRates.text = $"{presentor.MiningRate} \n {presentor.SpeedRate} \n {presentor.PackageRate} ";
            ImageManager.sprite = presentor.ImgManager;

            ImagePlanet.sprite = presentor.ImgPlanet;
            ImagePlanet.color = presentor.ImgPlanet == null ? Color.clear : Color.white;

            starGradeComp.Refresh(new StarGradeComp.PresentInfo(presentor.Level));
        }

        public void OnClicked()
        {
            EventOnBtnCardClicked?.Invoke(ManagerInfoId);
        }
    }
}
