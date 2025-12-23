using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class UpgradableSource
    {
        [SerializeField] protected string srcId;
        [SerializeField] protected int count;

        public string SrcId { get => srcId; set => srcId = value; }
        public int Count { get => count; set => count = value; }

        public UpgradableSource(string srcId, int cnt)
        {
            this.SrcId = srcId;   this.count = cnt;
        }
    }

    [Serializable]
    public class Upgradable
    {
        [SerializeField] protected string id;
        [SerializeField] protected List<UpgradableSource> sources = new List<UpgradableSource>();


        public string Id { get => id; set => id = value; }
        public List<UpgradableSource> Sources => sources;
        public void AddSource(UpgradableSource src)
        {
            Assert.IsNotNull(src);
            if(src != null)
                sources.Add(src);
        }
        public void RemoveSource(UpgradableSource src)
        {
            sources.Remove(src);
        }
    }
}
