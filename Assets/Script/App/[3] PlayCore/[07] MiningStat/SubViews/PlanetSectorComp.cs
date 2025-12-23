using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class PlanetSectorComp : IGCore.MVCS.AView
    {
        [SerializeField] Image PlanetIcon;
        [SerializeField] TMP_Text planetName;
        [SerializeField] TMP_Text ShotIntervalIconArea;
        [SerializeField] TMP_Text ShotAccuracyIconArea;
        [SerializeField] TMP_Text DeliverySpeedIconArea;
        [SerializeField] TMP_Text CargoSizeIconArea;
        [SerializeField] TMP_Text Distance;



        public class PresentInfo : APresentor
        {
            public PresentInfo()    // When there's no avaliable planet.
            {}

            public PresentInfo(Sprite _icon, string _name, string _shotInterval, string _shotAcc, string _speed, string _cargo, string _dist, string _lvMode)
            {
                Icon = _icon; Name = _name; ShotInterval = _shotInterval; ShotAccuracy = _shotAcc; DeliverySpeed = _speed; CargoSize = _cargo; Distance = _dist;
            }
            public Sprite Icon { get; private set; }
            public string Name { get; private set; }
            public string ShotInterval { get; private set; }
            public string ShotAccuracy { get; private set; }
            public string DeliverySpeed { get; private set; }
            public string CargoSize { get; private set; }
            public string Distance { get; private set; }
        }

        // Start is called before the first frame update
        private void Awake()
        {
            Assert.IsNotNull(PlanetIcon);
            Assert.IsNotNull(planetName);
            Assert.IsNotNull(Distance);
        }

        public override void Refresh(APresentor presentInfo)
        {
            if (presentInfo == null)
                return;

            PresentInfo presentor = (PresentInfo)presentInfo;

            PlanetIcon.sprite = presentor.Icon;
            planetName.text = presentor.Name;
            if (ShotIntervalIconArea != null) ShotIntervalIconArea.text = presentor.ShotInterval;
            if (ShotAccuracyIconArea != null) ShotAccuracyIconArea.text = presentor.ShotAccuracy;
            if (DeliverySpeedIconArea != null) DeliverySpeedIconArea.text = presentor.DeliverySpeed;
            if (CargoSizeIconArea != null) CargoSizeIconArea.text = presentor.CargoSize;
            Distance.text = presentor.Distance;
        }
    }
}