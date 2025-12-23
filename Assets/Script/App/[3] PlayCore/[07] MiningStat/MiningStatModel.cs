
using System.Numerics;
using UnityEngine;
using Core.Utils;
using System.Collections.Generic;
using Core.Events;
using System;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class MiningStatModel : IGCore.MVCS.AModel
    {
        #region ===> Properties.

        public MiningStatPlayerModel PlayerData => (MiningStatPlayerModel)playerData;        
        
        #endregion ===> Properties.



        #region ===> Interfaces

        public MiningStatModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) 
        { 
            // this.InitModel((string)ctx.GetData("planet_data_path"), (string)ctx.GetData("bossplanet_data_path"));
        }
        public override void Init() 
        { 
            _isInitialized = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            _isInitialized = false;
        }

        //==========================================================================
        //
        // Planet Controller
        //
        //
        

        

        #endregion ===> Interfaces




        #region ===> Helpers
        
        

        #endregion
    }
}
