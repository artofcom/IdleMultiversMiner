using System;
using UnityEngine;


namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class OptionDialog : APopupDialog
    {
        [SerializeField] ButtonToggle btnToggleSoundFX;
        [SerializeField] ButtonToggle btnToggleBGM;

        public static Action<bool> EventOnBtnBGMClicked;
        public static Action<bool> EventOnBtnSoundFXClicked;

        // Debug Actions.
        public static Action EventOnShowGameReset;

        public class PresentInfo : APresentor
        {
            public PresentInfo(bool isFXAudioOn, bool isBGMOn)
            {
                IsBGMOn = isBGMOn;
                IsFXAudioOn = isFXAudioOn;
            }

            public bool IsFXAudioOn { get; private set; }
            public bool IsBGMOn { get; private set; }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var presentInfo = (PresentInfo)presentor;
            if (presentInfo == null)
                return;

            btnToggleBGM.IsOn = presentInfo.IsBGMOn;
            btnToggleSoundFX.IsOn = presentInfo.IsFXAudioOn;
        }

        public void OnBtnSoundFXClicked(bool isOn)
        {
            EventOnBtnSoundFXClicked?.Invoke(isOn);
        }

        public void OnBtnBGMClicked(bool isOn)
        {
            EventOnBtnBGMClicked?.Invoke(isOn);
        }

        public void OnBtnGameResetClicked()
        {
            EventOnShowGameReset?.Invoke();
        }
    }
}