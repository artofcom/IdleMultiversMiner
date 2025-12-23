using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner.GamePlay
{
    public class MiningZoneComp : MonoBehaviour
    {
        [SerializeField] int zoneId;
        [SerializeField] List<PlanetBaseComp> planets;

        public int ZoneId => zoneId;
        public int PlanetCount => planets.Count;

#if UNITY_EDITOR
        public List<PlanetBaseComp> Planets => planets;
#endif

        public class BaseInfo
        {
            public BaseInfo(string _openCost)
            {
                IsOpened = false;
                OpenCost = _openCost;
            }
            public BaseInfo(string _name, float _shipSpeed, Sprite _sprManager)
            {
                IsOpened = true;
                Name = _name;
                ShipSpeed = _shipSpeed;
                SpriteManager = _sprManager;
            }

            public string Name { get; private set; }
            public bool IsOpened { get; private set; }
            public string OpenCost { get; private set; }
            public float ShipSpeed { get; private set; }
            public Sprite SpriteManager { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            UnityEngine.Assertions.Assert.IsTrue(planets != null && planets.Count > 0);
        }

        public void Init(GameObject mainCharObj, GameObject flyMonObjCache, List<BaseInfo> baseInfo)
        {
            UnityEngine.Assertions.Assert.IsTrue(baseInfo.Count == planets.Count);

            gameObject.SetActive(true);

            for (int q = 0; q < planets.Count; ++q)
            {
                int id = planets[q].PlanetId;
                planets[q].gameObject.SetActive(true);
                
                var info = baseInfo[ q ];
                if (planets[q].IsInitialized)
                    continue;

                switch(planets[q].Type)
                {
                    case PlanetData.KEY:
                        {
                            var comp = (planets[q] as PlanetComp);
                            comp.Init(info.IsOpened, info.ShipSpeed, mainCharObj, flyMonObjCache);

                            if (info.IsOpened)  comp.Refresh(new PlanetComp.PresentInfo(info.Name, info.ShipSpeed, info.SpriteManager, null));//  boosterEnabled:true, 0.6f));
                            else                comp.Refresh(new PlanetComp.PresentInfo(info.OpenCost));
                            break;
                        }
                    case PlanetBossData.KEY:
                        {
                            var comp = (planets[q] as PlanetBossComp);
                            comp.Init(info.IsOpened);

                            if (info.IsOpened) comp.Refresh(new PlanetBossComp.PresentInfo(info.Name, "", .0f, info.SpriteManager));
                            else               comp.Refresh(new PlanetBossComp.PresentInfo(info.OpenCost));
                            break;
                        }
                    default:
                        UnityEngine.Assertions.Assert.IsTrue(false, "Undefined Type !");
                        break;
                }
            }
        }

        public void ClearnUp()
        {
            Debug.Log($"<color=green>[Zone-CleanUp] Zone [{this.ZoneId}] clean up in progress..</color>");

            for (int q = 0; q < planets.Count; ++q)
            {
                switch(planets[q].Type)
                {
                    case PlanetData.KEY:
                        (planets[q] as PlanetComp).CleanUp();
                        break;
                    default:
                        UnityEngine.Assertions.Assert.IsTrue(false, "Unsupported Type.");
                        break;
                }
                planets[q].gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
        }

        public PlanetBaseComp GetPlanetComp(int planetId)
        {
            for (int q = 0; q < planets.Count; ++q)
            {
                if (planets[q].PlanetId == planetId)
                    return planets[q];
            }
            return null;
        }
        

        public List<Vector2> GetPlanetsPos()
        {
            List<Vector2> pos = new List<Vector2>();
            for (int q = 0; q < planets.Count; ++q)
            {
                pos.Add(planets[q].transform.position);
            }
            return pos;
        }
    }
}
