using UnityEngine;


[CreateAssetMenu(fileName = "AppConfig", menuName = "ScriptableObjects/AppConfig")]
public class AppConfig : ScriptableObject
{
    [SerializeField] int newPlayerIAP = 50;
    [SerializeField] int newPlayerStar = 0;

    [SerializeField] int maxServiceSignInWaitTime = 5;
    [SerializeField] int metaDataSaveLocalInterval = 2;
    [SerializeField] int metaDataSaveCloudInterval = 10;
    [SerializeField] int gameDataSaveLocalInterval = 1;
    [SerializeField] int gameDataSaveCloudInterval = 10;

    public int NewPlayerIAP => newPlayerIAP;
    public int NewPlayerStar => newPlayerStar;

    public int MaxServiceSignInWaitTime => maxServiceSignInWaitTime;
    public int MetaDataSaveLocalInterval => metaDataSaveLocalInterval;
    public int MetaDataSaveCloudInterval => metaDataSaveCloudInterval;
    public int GameDataSaveLocalInterval => gameDataSaveLocalInterval;
    public int GameDataSaveCloudInterval => gameDataSaveCloudInterval;

    void OnEnable() {}

}
