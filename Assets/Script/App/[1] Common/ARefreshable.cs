using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace App.GamePlay.IdleMiner
{
    public abstract class ARefreshable : MonoBehaviour
    {
        // Static Manager.
        //
        public static Dictionary<string, ARefreshable> RefreshableCache = new Dictionary<string, ARefreshable>();
        public static void Refresh(string id, IPresentor presentor)
        {
            if (RefreshableCache.ContainsKey(id))
            {
                var refTarget = RefreshableCache[id];
                refTarget.Refresh(presentor);
            }
        }
        public static void Refresh(ARefreshable target, IPresentor presentor)
        {
            UnityEngine.Assertions.Assert.IsNotNull(target);

            Refresh(target.Id, presentor);
        }





        // Refreshables.
        //
        public interface IPresentor
        { }
        public string Id { get; private set; }






        public virtual void Register(string id)
        {
            Id = id;
            if(RefreshableCache.ContainsKey(id))
            {
                Debug.LogError($"Refreshable [{id}] is already exists !!!");
                return;
            }
            RefreshableCache.Add(id, this);
            Debug.Log($"ARefreshable [{id}] has been added.");
        }
        private void OnDestroy()
        {
            if(!string.IsNullOrEmpty(Id) && RefreshableCache.ContainsKey(Id))
                RefreshableCache.Remove(Id);
        }

        public abstract void Refresh(IPresentor presentor);

    }
}
