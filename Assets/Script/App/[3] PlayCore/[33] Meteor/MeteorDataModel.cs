
using UnityEngine;
using System;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class MeteorData
    {
        [SerializeField] int rewardMultiply;
        [SerializeField] int showUpIntervalInMin;
     
        public int RewardMultiply => rewardMultiply;
        public int ShowUpIntervalInMin => showUpIntervalInMin;
    }


    public class MeteorModel : IGCore.MVCS.AModel
    {
        public MeteorData MeteorData { get; private set; }

        public MeteorModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

        public override void Init() { }
        /*
        void InitMeteor()
        {
            var textData = Resources.Load<TextAsset>(GAMEDATA_PATH + "MeteorData");
            MeteorData = JsonUtility.FromJson<MeteorData>(textData.text);

            Assert.IsNotNull(MeteorData);
        }*/
    }
}
