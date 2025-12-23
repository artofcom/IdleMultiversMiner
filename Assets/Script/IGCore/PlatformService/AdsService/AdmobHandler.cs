using GoogleMobileAds.Api;
using System;
using UnityEngine;

namespace Core.Platform
{
    public class AdmobHandler : MonoBehaviour
    {
        // These ad units are configured to always serve test ads.
    #if UNITY_ANDROID
        private string _adUnitId = "ca-app-pub-6855264133583551/1722206438";// "ca-app-pub-3940256099942544/5224354917";
    #elif UNITY_IPHONE
        private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
    #else
        private string _adUnitId = "unused";
    #endif

        private RewardedAd _rewardedAd = null;


        // Start is called before the first frame update
        void Start()
        {
            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(initStatus => 
            {
                LoadRewardedAd();            
            });
        }

        private void OnDestroy()
        {
            if (_rewardedAd != null)
                _rewardedAd.Destroy();
        }

        
        public void DisplayAds(Action callbackRewardEarned)
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                Debug.Log("Displaying Ads....");
                _rewardedAd.Show((Reward reward) =>
                {
                    // TODO: Reward the user.
                    Debug.Log($"Rewarded ad rewarded the user. Type: {reward.Type}, amount: {reward.Amount}.");

                    callbackRewardEarned?.Invoke();
                });
            }
            else Debug.Log("Note : Displaying Ads is not ready.");
        }


        /// <summary>
        /// Loads the rewarded ad.
        /// </summary>
        void LoadRewardedAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            Debug.Log("Loading the rewarded ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                _rewardedAd = ad;

                RegisterEventHandlers(_rewardedAd);
            });
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(string.Format("Rewarded ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                LoadRewardedAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " + "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                LoadRewardedAd();
            };
        }
    }
}

/*
        public void OnBtnGoClicked()
        {
            const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show((Reward reward) =>
                {
                    // TODO: Reward the user.
                    Debug.Log(string.Format(rewardMsg, reward.Type, reward.Amount));
                });
            }
        }*/