using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace IGCore.MVCS
{
    public abstract class AUnitSwitcher : MonoBehaviour
    {
        /*
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

        protected virtual void Init(IGCore.MVCS.AContext ctx)
        {
            foreach(var module in units)
                module.Init(ctx);

            foreach(var module in units)
            {
                if(module == startUnit)
                {
                    module.Attach();
                    module.OnEventClose += OnEventClose;
                }
                else 
                    module.Detach();
            }
        }

        protected virtual void OnEventClose(string nextModuleId)
        {
            foreach(var module in units)
            {
                // Debug.Log("====" + module.GetType().Name.ToLower() + "____" + nextModuleId.ToLower());
                // module.View.gameObject.SetActive();

                module.OnEventClose -= OnEventClose;
                bool isTargetModule = module.GetType().Name.ToLower().Contains(nextModuleId.ToLower());
                if(isTargetModule)
                {
                    module.Attach();
                    module.OnEventClose += OnEventClose;
                }
                else                    
                    module.Detach();
            }
        }*/
    }
}
