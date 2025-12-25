using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.GamePlay.IdleMiner.Common.PlayerModel
{
    [Serializable]
    public class NotificationInfo
    {
        [SerializeField] List<string> seenReasons;
        [SerializeField] List<string> unseenReasons;

        public List<string> SeenReasons { get => seenReasons; set => seenReasons = value; }
        public List<string> UnseenReasons { get => unseenReasons; set => unseenReasons = value; }

        public void Sanitize()
        {
            if(seenReasons == null)     seenReasons = new List<string>();
            if(unseenReasons == null)   unseenReasons = new List<string>();
        }
    }
}