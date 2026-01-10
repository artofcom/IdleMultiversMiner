using System;

namespace IGCore.MVCS
{
    public interface IUnitSwitcher 
    {
        event Action OnPreSwitch;
        event Action OnPostSwitch;

        void SwitchUnit(string nextUnitName, object data);
    }
}
