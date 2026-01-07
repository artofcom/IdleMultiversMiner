using System;

namespace IGCore.PlatformService.Cloud
{
    public interface IService 
    {
        bool IsInitialized();

        event Action EventOnInitialized;
    }
}
