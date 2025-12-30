using System;
using TMPro;
using UnityEngine;


namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class SettingDialogView : APopupDialog
    {
        [SerializeField] ButtonToggle btnToggleSoundFX;
        [SerializeField] ButtonToggle btnToggleBGM;
        [SerializeField] Transform SoundFXListRoot;
        [SerializeField] TMP_Text txtPlayerId;
        [SerializeField] Transform BGMListRoot;
        [SerializeField] TMP_Text txtVersion;

        [SerializeField] GameObject OfflineRoot;
        [SerializeField] GameObject OnlineRoot;
        [SerializeField] GameObject LinkAccountRoot;
        [SerializeField] GameObject UnlinkAccountRoot;
        [SerializeField] GameObject SignOutRoot;
        [SerializeField] GameObject DeleteAccountRoot;
        

        public static Action<bool> EventOnBtnBGMClicked;
        public static Action<bool> EventOnBtnSoundFXClicked;
        public static Action EventOnLinkAccountClicked;
        public static Action EventOnUnlinkAccountClicked;
        public static Action EventOnSignOutClicked;
        public static Action EventOnDeleteAccountClicked;
        public static Action EventOnAccountManagementClicked;

        // Debug Actions.
        public static Action EventOnShowGameReset;

        public class PresentInfo : APresentor
        {
            public PresentInfo(bool isFXAudioOn, bool isBGMOn, bool isSignedIn, bool isLinked, string playerId, string strVersion)
            {
                IsBGMOn = isBGMOn;
                IsFXAudioOn = isFXAudioOn;
                IsSignedIn = isSignedIn;
                IsLinked = isLinked;    
                PlayerId = playerId;
                Version = strVersion;
            }

            public bool IsFXAudioOn { get; private set; }
            public bool IsBGMOn { get; private set; }
            
            public bool IsSignedIn { get; private set; }
            public bool IsLinked { get; private set; }
            public string PlayerId { get; private set; }
            public string Version { get; private set; }
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

            OfflineRoot.SetActive(!presentInfo.IsSignedIn);
            OnlineRoot.SetActive(presentInfo.IsSignedIn);

            btnToggleBGM.IsOn = presentInfo.IsBGMOn;
            btnToggleSoundFX.IsOn = presentInfo.IsFXAudioOn;
            
            LinkAccountRoot.SetActive(!presentInfo.IsLinked);
            UnlinkAccountRoot.SetActive(presentInfo.IsLinked);

            DeleteAccountRoot.SetActive(!presentInfo.IsLinked);
            SignOutRoot.SetActive(presentInfo.IsLinked);

            txtPlayerId.text = presentInfo.PlayerId;
            txtVersion.text = presentInfo.Version;
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

        public void OnBtnLinkAccountClicked()
        {
            EventOnLinkAccountClicked?.Invoke();
        }
        public void OnBtnUnlinkAccountClicked()
        {
            EventOnUnlinkAccountClicked?.Invoke();
        }
        public void OnBtnSignOutClicked()
        {
            EventOnSignOutClicked?.Invoke();
        }
        public void OnBtnDeleteAccountClicked()
        {
            EventOnDeleteAccountClicked?.Invoke();
        }
        public void OnBtnAccountManagement()
        {
            EventOnAccountManagementClicked?.Invoke();
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