using UnityEngine;

namespace IGCore.Components
{
    // Powered by Gemini & artofcom.
    public class NotificatorComp : MonoBehaviour, INotificator
    {
        [SerializeField] GameObject objectNotifier;

        private void Awake()
        {
            if(objectNotifier != null) 
                objectNotifier.SetActive(false);
        }

        // returns if the new reason is valid.
        public bool EnableNotification(string reason_key_no_use)
        {
            if(objectNotifier != null) 
                objectNotifier.SetActive(true);

            return true;
        }

        // When player opens the UI - notifier needs to be cleared.
        public void DisableNotification()
        {
            if(objectNotifier != null) 
                objectNotifier.SetActive(false);
        }

        public void Reset() {}
    }
}
