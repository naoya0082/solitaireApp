using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;

public class AdMobBanner : MonoBehaviour
{

    private BannerView bannerView;
    public static AdMobBanner instance;

    public void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        // Google AdMob Initial
        MobileAds.Initialize(initStatus => { });
        this.RequestBanner();
    }

    private void RequestBanner()
    {
#if   UNITY_ANDROID
        //string adUnitId = "ca-app-pub-3940256099942544/6300978111"; // 試験用
        string adUnitId = "ca-app-pub-8673262984895359/1510130747"; // リリース用

#elif UNITY_IPHONE
        //string adUnitId = "ca-app-pub-3940256099942544/2934735716"; // 試験用
        string adUnitId = "ca-app-pub-8673262984895359/9332780448"; // リリース用
#else
        string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the bottom of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }
}