using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace IGCore.MVCS
{
    public class UnitSwitcherComp : MonoBehaviour, IUnitSwitcher
    {
        public event Action OnPreSwitch;
        public event Action OnPostSwitch;

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

        public virtual async void SwitchUnit(string nextModuleId, object data)
        {
            OnPreSwitch?.Invoke();

            await Task.Delay(100);

            foreach(var module in units)
            {
                bool isTargetModule = module.GetType().Name.ToLower().Contains(nextModuleId.ToLower());
                if(isTargetModule)
                    module.Attach();
                else                    
                    module.Detach();
            }

            await Task.Delay(100);

            OnPostSwitch?.Invoke();
        }
    }
}