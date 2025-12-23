using TMPro;
using UnityEngine;


namespace IGCore.Components
{
    public class TimeScaleHandler : MonoBehaviour
    {
        [SerializeField] TMP_Text textValue;
        [SerializeField] bool saveTimeScale = false;

        const string SaveKey = "LastTimeScale";

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if(saveTimeScale)
            {
                float lastTS = PlayerPrefs.GetFloat(SaveKey, 1.0f);
                Time.timeScale = Mathf.Clamp(lastTS, 1.0f, 10.0f);
            }

            if(textValue != null)
                textValue.text = Time.timeScale.ToString("0.00");
        }

        public void OnBtnChangeSpeed(float offset)
        {
            Time.timeScale += offset;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 1.0f, 10.0f);

            if(textValue != null)
                textValue.text = Time.timeScale.ToString("0.00");

            if(saveTimeScale) 
                PlayerPrefs.SetFloat(SaveKey, Time.timeScale);
            
            Debug.Log($"[TimeScaleHandler] TS has been changed to {Time.timeScale}");
        }
    }
}
