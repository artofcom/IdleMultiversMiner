using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using System; 

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class GameCardsPortalDialog : APopupDialog
    {
        public enum Type { CONFIRM, YES_NO, };

        // Serialize Fields -------------------------------
        //
        [SerializeField] GameCardsView portalComp;
        
        Action<string> OnCardClicked;

        public class PresentInfo : APresentor
        {            
            public PresentInfo(string message, GameCardsView.Presentor cardsPresentor, Action<string> onCardClicked)
            {
                Message = message;
                this.cardsPresentor = cardsPresentor;
                OnCardClicked = onCardClicked;
            }

            public string Message { get; private set; }
            public GameCardsView.Presentor cardsPresentor { get; private set; }
            public Action<string> OnCardClicked { get; private set; }
        }

        PresentInfo curPresentInfo = null;

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(portalComp);
            portalComp.EventGameCardClicked += OnBtnGameCardClicked;
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var presentInfo = (PresentInfo)presentor;
            if (presentInfo == null)
                return;

            curPresentInfo = presentInfo;

            OnCardClicked += presentInfo.OnCardClicked;
            portalComp.Refresh(presentInfo.cardsPresentor);
        }

        public void OnCloseBtnClicked()
        {
            OnCardClicked -= curPresentInfo.OnCardClicked;
            OnClose();
        }

        public void OnBtnGameCardClicked(string game_key)
        {
            OnCardClicked?.Invoke(game_key);
            OnCardClicked -= curPresentInfo.OnCardClicked;
            OnClose();
        }
    }
}
