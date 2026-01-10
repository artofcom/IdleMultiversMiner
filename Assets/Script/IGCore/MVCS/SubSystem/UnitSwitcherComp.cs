using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace IGCore.MVCS
{
    public class UnitSwitcherComp : MonoBehaviour, IUnitSwitcher
    {
        [SerializeField] protected List<AUnit> units;
        [SerializeField] protected AUnit startUnit;

        protected virtual void Awake()
        {
            Assert.IsNotNull(units);
            Assert.IsTrue(units.Count > 0);
        }

        protected void OnEnable()
        {
            if(startUnit == null)
                startUnit = units[0];
        }

        public virtual void Init(AContext ctx)
        {
            foreach(var module in units)
                module.Init(ctx);

            foreach(var module in units)
            {
                if(module == startUnit)
                    module.Attach();
                else 
                    module.Detach();
            }
        }

        protected virtual void Detach(string nextModuleId)
        {
            foreach(var module in units)
            {
                bool isTargetModule = module.GetType().Name.ToLower().Contains(nextModuleId.ToLower());
                if(!isTargetModule)                    
                    module.Detach();
            }
        }

        protected virtual void Attach(string nextModuleId)
        {
            foreach(var module in units)
            {
                bool isTargetModule = module.GetType().Name.ToLower().Contains(nextModuleId.ToLower());
                if(isTargetModule)
                    module.Attach();
            }
        }

        public virtual void SwitchUnit(string nextModuleId, object data)
        {
            Detach(nextModuleId);

            Attach(nextModuleId);
        }
    }
}