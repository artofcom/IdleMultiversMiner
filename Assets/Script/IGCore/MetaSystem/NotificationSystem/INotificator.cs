

namespace IGCore.Components
{
    public interface INotificator
    {
        bool EnableNotification(string condition_id);
        
        void DisableNotification();

        void Reset();
    }
}