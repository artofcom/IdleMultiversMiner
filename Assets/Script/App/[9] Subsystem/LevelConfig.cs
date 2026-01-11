using UnityEngine;


[CreateAssetMenu(fileName = "LevelConfig", menuName = "ScriptableObjects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [SerializeField] string levelName = "Lost Desert v1.";
    [SerializeField] string expectedClearDate = "28d 5h";

    public string LevelName => levelName;
    public string ExpectedClearDate => expectedClearDate;

    void OnEnable() {}

}
