using UnityEngine;


[CreateAssetMenu(fileName = "GameConfig", menuName = "ScriptableObjects/GameConfig")]
public class GameConfig : ScriptableObject
{
    [SerializeField] int timedBonusIntervalInMin = 30;
    [SerializeField] float timedBonusRewardRatio = 50.0f;
    [SerializeField] float speedOfMiningBall = 50.0f;

    public int TimedBonusIntervalInMin => timedBonusIntervalInMin;
    public float TimedBonusRewardRatio => timedBonusRewardRatio;
    public float SpeedOfMiningBall => speedOfMiningBall; 

    void OnEnable() {}

}
