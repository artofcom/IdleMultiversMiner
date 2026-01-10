using System;

namespace IGCore.MVCS
{
    public interface IUnitSwitcher 
    {
        void SwitchUnit(string nextUnitName, object data);
    }
}
