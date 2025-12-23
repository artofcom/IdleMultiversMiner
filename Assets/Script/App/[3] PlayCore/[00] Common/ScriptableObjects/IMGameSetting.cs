using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameSetting", order = 1)]
public class IMGameSetting : ScriptableObject
{
    [SerializeField] string resourceMatPath;
    [SerializeField] string resourceCompPath;
    [SerializeField] string resourceItemPath;
    [SerializeField] string planetDataPath;
    [SerializeField] string planetBossDataPath;
    [SerializeField] string compCraftDataPath;
    [SerializeField] string itemCraftDataPath;
    [SerializeField] List<string> projectDataPath;



    public string ResourceMatPath => resourceMatPath;
    public string ResourceCompPath => resourceCompPath;
    public string ResourceItemPath => resourceItemPath;
    public string PlanetsDataPath => planetDataPath;
    public string PlanetBossDataPath => planetBossDataPath;
    public string CompCraftDataPath => compCraftDataPath;

    public string ItemCraftDataPath => itemCraftDataPath;
    public string ProjectDataPath(int idx)
    {
        if(idx>=0 && idx<projectDataPath.Count) return projectDataPath[idx];
        return string.Empty;
    }

}
