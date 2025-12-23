using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Core.Events;
using TMPro;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class WelcomeBackPopupDialog : APopupDialog
    {
        // const string ------------------------------------
        //

        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text TextMessage;
        [SerializeField] TMP_Text TextProgress;
        [SerializeField] GameObject LoadingObject;



        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        public class PresentInfo : APresentor
        {
            public PresentInfo(string _msg, string _prog, bool isLoading)
            {
                Message = _msg;  Progress = _prog;  IsLoading = isLoading;      
            }

            public string Message { get; private set; }
            public string Progress { get; private set; }
            public bool IsLoading { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(TextMessage);
            Assert.IsNotNull(TextProgress);
            Assert.IsNotNull(LoadingObject);
        }

        public override void InitDialog(string id)
        {
            base.InitDialog(id);
            sID = id;
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var presentInfo = (PresentInfo)presentor;
            if (presentInfo == null)
                return;

            TextMessage.text = presentInfo.Message;
            TextProgress.text = "0%";// presentInfo.Progress;
            LoadingObject.SetActive(true);

            StartCoroutine(TriggerActionWithDelay(0.1f, () =>
            {
                StartCoroutine(coIncreaseCharger());

            }) );
        }

        IEnumerator coIncreaseCharger()
        {
            const float fDuration = 1.0f;

            float fStartTime = Time.time;
            while(Time.time-fStartTime < fDuration)
            {
                TextProgress.text = $"{100.0f * (Time.time - fStartTime) / fDuration}%";
                yield return null;
            }

            TextProgress.text = "100%";
            LoadingObject.SetActive(false);
        }

        IEnumerator TriggerActionWithDelay(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);

            action?.Invoke();
        }


        public void OnCloseBtnClicked()
        {
            OnClose();
        }
    }
}
