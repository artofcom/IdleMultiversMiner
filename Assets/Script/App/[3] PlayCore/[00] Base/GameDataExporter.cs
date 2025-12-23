using App.GamePlay.IdleMiner.GamePlay;
using App.GamePlay.IdleMiner.SkillTree;
using UnityEngine;

public class GameDataExporter : MonoBehaviour
{
    [SerializeField] ResourceDataBuildComp resourceDataBuildComp;
    [SerializeField] PlanetControllerComp zoneDataComp;
    [SerializeField] CraftDataBuildComp craftCompDataBuildComp;
    [SerializeField] CraftDataBuildComp craftItemDataBuildComp;
    [SerializeField] SkillItemBundleComp skillItemBundleComp;

    public ResourceDataBuildComp ResourceDataBuildComp => resourceDataBuildComp;
    public PlanetControllerComp ZoneDataComp => zoneDataComp;
    public CraftDataBuildComp CraftCompDataBuildComp => craftCompDataBuildComp;
    public CraftDataBuildComp CraftItemDataBuildComp => craftItemDataBuildComp;
    public SkillItemBundleComp SkillItemBundleComp => skillItemBundleComp;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {  
    }
}
 