using System;
using UnityEngine;


namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class SettingDialogView : APopupDialog
    {
        [SerializeField] ButtonToggle btnToggleSoundFX;
        [SerializeField] ButtonToggle btnToggleBGM;
        [SerializeField] Transform SoundFXListRoot;

        public static Action<bool> EventOnBtnBGMClicked;
        public static Action<bool> EventOnBtnSoundFXClicked;

        // Debug Actions.
        public static Action EventOnShowGameReset;


        [SerializeField] Transform BGMListRoot;

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




        public void EnableSoundFX(bool enable)
        {
            if(SoundFXListRoot == null) return;

            for(int q = 0; q < SoundFXListRoot.childCount; ++q)
            {
                var audio = SoundFXListRoot.GetChild(q).GetComponent<AudioSource>();
                if(audio == null) continue;

                audio.volume = enable ? 0.9f : .0f;
            }
        }

        public void EnableBGM(bool enable)
        {
            if(BGMListRoot == null) return;

            for(int q = 0; q < BGMListRoot.childCount; ++q)
            {
                var audio = BGMListRoot.GetChild(q).GetComponent<AudioSource>();
                if(audio == null) continue;

                audio.volume = enable ? 0.9f : .0f;
            }
        }
    }
}