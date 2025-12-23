using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Threading.Tasks;
using GAME = App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner;

[Serializable]
public class TimeUpgradable : GAME.Upgradable
{
    [SerializeField] float obtainTime;
    public float ObtainTime { get => obtainTime; set => obtainTime = value; }

    public TimeUpgradable(string id, float obtainTime)
    {
        this.id = id;   this.obtainTime = obtainTime;
    }
    public bool IsUsed { get; set; } = false;
}

[Serializable]
public class TimeUpgradableGroup
{
    public enum Type { MAT, COMP, ITEM, PROJECT, MAX };

    [SerializeField] List<TimeUpgradable> upgradables = new List<TimeUpgradable>();
    [SerializeField] int type;

    public List<TimeUpgradable> Upgradables => upgradables;
    public Type eType => (Type)type;

    public TimeUpgradableGroup(Type _type)
    {
        type = (int)_type;
    }
    public void AddUpgradable(TimeUpgradable TimeUpgradable, int idxAt = -1)
    {
        if(Upgradables.Count>0 && idxAt>=0)
            Upgradables.Insert(idxAt, TimeUpgradable);
        else
            Upgradables.Add(TimeUpgradable);
        
    }
    public void DeleteUpgradable(string id)
    {
        TimeUpgradable enf = GetUpgradable(id);
        if(enf != null)
            Upgradables.Remove(enf);
    }
    public TimeUpgradable GetUpgradable(string id)
    {
        for(int q = 0; q < Upgradables.Count; ++q)
        {
            if(Upgradables[q].Id == id)
                return Upgradables[q];
        }
        return null;
    }
}


public class ResourceCraftEditor : EditorWindow
{
    const int AUTO_SAVE_SEC = 30;
    const int WIDTH = 2000;
    const int LANE_W = 500;
    const string DATA_SUB_PATH = "/EditorData/CraftData";

    List<TimeUpgradableGroup> EnforceGroups = new List<TimeUpgradableGroup>();

    string mOutputPath = string.Empty;
    private GUIStyle styleRed;
    Vector2 vScrollPos = Vector2.zero;
    bool IsAutoSave = false;
    int mSecToSave = AUTO_SAVE_SEC;
    List<List<string>> RefIdList = new List<List<string>>();
    List<bool> IsUseOutputFileAsSrcForCraftData = new List<bool>();
    string ExportPath => mOutputPath + "/Export";

    [MenuItem("PlasticGames/Editor/CraftEditor")]
    public static void ShowWindow()
    {
        GetWindow<ResourceCraftEditor>().Init();
    }

    public void Init()
    {
        // make up temp data. 
        EnforceGroups.Clear();

        AutoSaveCountDown();        
    }

    void AutoSaveCountDown(bool reset = false)
    {
        if(reset)   mSecToSave = AUTO_SAVE_SEC;

        if(!IsAutoSave) return;

        TriggerActionWithDelayAsync(1000, () =>
        {
            --mSecToSave;
            if(mSecToSave < 0)
            {
                Save();
                mSecToSave = AUTO_SAVE_SEC;
            }
            Repaint();

            AutoSaveCountDown();
        });
    }

    void OnEnable()
    {
    }

    public void OnGUI()
    {
        // init.
        mOutputPath = Application.dataPath + DATA_SUB_PATH;
        // Application.dataPath : c:/xxx/aaa/Assets
        if (IsUseOutputFileAsSrcForCraftData.Count == 0)
        {
            for (int q = 0; q < (int)TimeUpgradableGroup.Type.MAX; ++q)
                IsUseOutputFileAsSrcForCraftData.Add(false);
        }

        if (styleRed == null)
        {
            TriggerActionWithDelayAsync(100, () =>
            {
                styleRed = new GUIStyle(EditorStyles.label);
                styleRed.normal.textColor = Color.red;
            });
        }

        GUILayout.Label("[Craft Editor] : 19-26-33-50+");

        DrawHeaderGroup();

        DrawUpgradables();

        

        // Dummy and Tests.
        GUILayout.Space(10);
       // GUILayout.Label("=== Numbering for less confusing");
        GUILayout.Label("=== Missing link check");
    }

    void DrawHeaderGroup()
    {
        bool oldFlag = IsAutoSave;
        IsAutoSave = GUILayout.Toggle(oldFlag, "Auto Save", GUILayout.Width(100));
        if (!oldFlag && IsAutoSave)
            AutoSaveCountDown(reset: true);
        GUILayout.Label("Output Path : " + mOutputPath);

        GUILayout.BeginHorizontal(GUILayout.Width(WIDTH));
        if (GUILayout.Button("Load"))
        {
            this.Init();
            this.Load();
        }
        string strBtn = IsAutoSave ? $"Save ({(int)mSecToSave})" : "Save";
        if (GUILayout.Button(strBtn)) { this.Save(); mSecToSave = AUTO_SAVE_SEC; }
        if (GUILayout.Button("Evaluate")) { Evaluate(); }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal(GUILayout.Width(WIDTH));
        {
            for (int group = 0; group < EnforceGroups.Count; ++group)
            {
                TimeUpgradableGroup enfGroup = EnforceGroups[group];
                GUILayout.BeginHorizontal(GUILayout.Width(LANE_W));
                {
                    GUILayout.Label($"{((TimeUpgradableGroup.Type)group)} : {enfGroup.Upgradables.Count}");
                    if (GUILayout.Button("Add"))//, GUILayout.Width(300)))
                        enfGroup.AddUpgradable(new TimeUpgradable("Id", 10.0f));
                }
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndHorizontal();

        TimeUpgradableGroup.Type eType;
        GUILayout.BeginHorizontal(GUILayout.Width(WIDTH));
        for (int group = 0; group < EnforceGroups.Count; ++group)
        {
            eType = (TimeUpgradableGroup.Type)group;
            switch ((TimeUpgradableGroup.Type)group)
            {
                case TimeUpgradableGroup.Type.COMP:
                case TimeUpgradableGroup.Type.ITEM:
                case TimeUpgradableGroup.Type.MAT:
                    if (GUILayout.Button($"Export to {eType}ResourceData", GUILayout.Width(LANE_W)))
                        ExportToResource(eType);
                    break;
                default:
                    if (GUILayout.Button("N/A", GUILayout.Width(500))) { }
                    break;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(WIDTH));
        for (int group = 0; group < EnforceGroups.Count; ++group)
        {
            eType = (TimeUpgradableGroup.Type)group;
            switch ((TimeUpgradableGroup.Type)group)
            {
                case TimeUpgradableGroup.Type.COMP:
                case TimeUpgradableGroup.Type.ITEM:
                    {
                        string filePath = ExportPath + $"/Craft_{eType.ToString()}.json";
                        int half = LANE_W / 2;
                        if (GUILayout.Button($"Export to {eType}CraftData", GUILayout.Width(half + 10)))
                            ExportToCraftData(eType, filePath, IsUseOutputFileAsSrcForCraftData[group]);
                        oldFlag = IsUseOutputFileAsSrcForCraftData[group];
                        IsUseOutputFileAsSrcForCraftData[group] = GUILayout.Toggle(IsUseOutputFileAsSrcForCraftData[group], "Use Output as Source", GUILayout.Width(half - 10));
                        break;
                    }
                case TimeUpgradableGroup.Type.PROJECT:
                    {
                        if (GUILayout.Button($"Export to Project(SkillTree)Data"))
                            ExportToProjectData();
                        break;
                    }
                default:
                    if (GUILayout.Button("N/A", GUILayout.Width(LANE_W))) { }
                    break;
            }
        }
        GUILayout.EndHorizontal();
    }

    void DrawUpgradables()
    {
        vScrollPos = GUILayout.BeginScrollView(vScrollPos);
        GUILayout.BeginHorizontal(GUILayout.Width(WIDTH));
        {
            for (int group = 0; group < EnforceGroups.Count; ++group)
            {
                GUILayout.BeginVertical();
                {
                    TimeUpgradableGroup enfGroup = EnforceGroups[group];

                    DrawTimeUpgradableGroup(enfGroup, group);
                }
                GUILayout.EndVertical();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
    }

    void DrawTimeUpgradableGroup(TimeUpgradableGroup upgradableGroup, int group)
    {
        string strField;

        for (int k = 0; k < upgradableGroup.Upgradables.Count; ++k)
        {
            GUILayout.Space(30);

            TimeUpgradable enf = upgradableGroup.Upgradables[k];
            GUILayout.BeginHorizontal(GUILayout.Width(LANE_W));
            {
                if (GUILayout.Button("U", GUILayout.Width(30)))
                {
                    if (k > 0)
                    {
                        upgradableGroup.Upgradables.RemoveAt(k);
                        upgradableGroup.Upgradables.Insert(k - 1, enf);
                    }
                }
                if (GUILayout.Button("D", GUILayout.Width(30)))
                {
                    if (k < upgradableGroup.Upgradables.Count - 1)
                    {
                        upgradableGroup.Upgradables.RemoveAt(k);
                        upgradableGroup.Upgradables.Insert(k + 1, enf);
                    }
                }
                if (group == 0)
                {
                    if (GUILayout.Button("Check Ref", GUILayout.Width(70)))
                        CollectReferenceId(upgradableGroup.Upgradables[k].Id);
                }
            }
            GUILayout.EndHorizontal();
            //
            GUILayout.BeginHorizontal(GUILayout.Width(LANE_W));
            {
                GUILayout.Label(string.Format("{0:D3})", k + 1), GUILayout.Width(50));

                if (IsReferencedId(group, enf.Id))
                    strField = GUILayout.TextField(enf.Id, styleRed);
                else
                    strField = GUILayout.TextField(enf.Id);

                if (strField != enf.Id) enf.Id = strField;
                GUILayout.Label("MiningTime", GUILayout.Width(90));
                if (group > 0) GUILayout.Label(SecToTime((long)enf.ObtainTime));
                else
                {
                    strField = GUILayout.TextField(enf.ObtainTime.ToString());
                    if (strField != enf.ObtainTime.ToString())
                    {
                        float outValue;
                        if (float.TryParse(strField, out outValue))
                            enf.ObtainTime = outValue;
                    }
                }

                if (group > 0)
                {
                    if (GUILayout.Button("Add Src", GUILayout.Width(60)))
                        enf.AddSource(new GAME.UpgradableSource("newSrc", 10));
                }
            }
            GUILayout.EndHorizontal();

            for (int s = 0; s < enf.Sources.Count; ++s)
            {
                GAME.UpgradableSource enfSrc = enf.Sources[s];

                GUILayout.BeginHorizontal(GUILayout.Width(LANE_W));
                {
                    GUILayout.Space(20);
                    strField = GUILayout.TextField(enfSrc.SrcId);
                    if (strField != enfSrc.SrcId) enfSrc.SrcId = strField;
                    GUILayout.Label("CNT ", GUILayout.Width(30));
                    strField = GUILayout.TextField(enfSrc.Count.ToString());
                    if (strField != enfSrc.Count.ToString())
                    {
                        int outValue;
                        if (int.TryParse(strField, out outValue))
                            enfSrc.Count = outValue;
                    }

                    float rscMiningTime = GetObtainSec(enfSrc.SrcId, false);
                    if (rscMiningTime < .0f) GUILayout.Label("No Ref !", styleRed);
                    else
                    {
                        rscMiningTime = rscMiningTime * ((float)enfSrc.Count);

                        if (rscMiningTime < 1.0f)
                            GUILayout.Label(rscMiningTime.ToString() + "s");
                        else GUILayout.Label(SecToTime((long)rscMiningTime));
                    }

                    if (group > 0)
                    {
                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                            enf.RemoveSource(enfSrc);
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(GUILayout.Width(LANE_W));
            {
                if (!IsResourceUsed((TimeUpgradableGroup.Type)group, enf.Id)) //q<EnforceGroups.Count-1 && !enf.IsUsed)
                    GUILayout.Label("Missing Reference!!!", styleRed);
                if (IsIdDuplicated((TimeUpgradableGroup.Type)group, enf.Id))
                    GUILayout.Label("Id duplicated!!!", styleRed);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(LANE_W));
            {
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    upgradableGroup.DeleteUpgradable(enf.Id);
                if (group == 0)
                    GUILayout.Label($"Mining Rate : {1.0f / enf.ObtainTime}/s");
                if (GUILayout.Button("Add Next", GUILayout.Width(70)))
                    upgradableGroup.AddUpgradable(new TimeUpgradable("Id", 10.0f), k + 1);
            }
            GUILayout.EndHorizontal();
        }
    }

    void Load()
    {
        for(int q = 0; q < (int)TimeUpgradableGroup.Type.MAX; ++q)
        {
            bool successed = false;
            string target = mOutputPath + $"/{((TimeUpgradableGroup.Type)q).ToString()}_rsc.json";
            if(File.Exists(target))
            {
                string data = File.ReadAllText(target);
                if(!string.IsNullOrEmpty(data))
                {
                    successed = true;
                    EnforceGroups.Add( JsonUtility.FromJson<TimeUpgradableGroup>(data) );
                }
            }

            if(!successed)
                EnforceGroups.Add( new TimeUpgradableGroup((TimeUpgradableGroup.Type)q) );
        }     
    }

    void Save()
    {
        for(int q = 0; q < EnforceGroups.Count; ++q)
        {
            WriteFile(mOutputPath + $"/{((TimeUpgradableGroup.Type)q).ToString()}_rsc.json", EnforceGroups[q]);
        }     
    }

    void ExportToResource(TimeUpgradableGroup.Type eType)
    {
        GAME.ResourceData rscData = new GAME.ResourceData();
        int idx = (int)eType;
        for(int q = 0; q < EnforceGroups[idx].Upgradables.Count; ++q)
        {
            GAME.ResourceInfo info = new GAME.ResourceInfo(EnforceGroups[idx].Upgradables[q].Id);
            rscData.Data.Add(info);
        }
        WriteFile(ExportPath + $"/Resource_{eType.ToString()}.json", rscData);
    }

    void ExportToProjectData()
    {
      /*  int idx = (int)TimeUpgradableGroup.Type.PROJECT;
        List<TimeUpgradable> listItems = EnforceGroups[idx].Upgradables;
        List<GAME.SkillInfo> projList = new List<SkillInfo>();
        for (int q = 0; q < listItems.Count; ++q)
        {
            List<GAME.UpgradableSource> listSrc = listItems[q].Sources;
            List<GAME.ResourceRequirement> listReq = new List<GAME.ResourceRequirement>();
            for (int s = 0; s < listSrc.Count; ++s)
                listReq.Add(new GAME.ResourceRequirement(listSrc[s].SrcId, listSrc[s].Count));

           // GAME.SkillInfo info = new GAME.SkillInfo("", "", "", listReq);
           // projList.Add(info);
        }
      */
        //WriteFile(ExportPath + $"/Project.json", new GAME.SkillData(projList));
    }

    void ExportToCraftData(TimeUpgradableGroup.Type eType, string filePath, bool isUseOtherDataFromSource)
    {
        int idx = (int)eType;
        GAME.CraftData craftData = null;
        if (isUseOtherDataFromSource && File.Exists(filePath))
        {
            string data = File.ReadAllText(filePath);
            craftData = JsonUtility.FromJson<CraftData>(data);
            if(craftData.Recipes.Count != EnforceGroups[idx].Upgradables.Count)
            {
                Debug.LogWarning("Can't use source cuz the recipe count is different.");
                craftData = null;
            }
        }

        List<TimeUpgradable> listItems = EnforceGroups[idx].Upgradables;
        List<GAME.RecipeInfo> listRecipe = new List<GAME.RecipeInfo>();
        for (int q = 0; q < listItems.Count; ++q)
        {
            List<GAME.UpgradableSource> listSrc = listItems[q].Sources;
            List<GAME.ResourceRequirement> listReq = new List<GAME.ResourceRequirement>();
            for(int s = 0; s < listSrc.Count; ++s)
                listReq.Add( new GAME.ResourceRequirement(listSrc[s].SrcId, listSrc[s].Count) );
            


            if(craftData == null)
                listRecipe.Add(new GAME.RecipeInfo($"id_{eType}_{q + 1}", listItems[q].Id, listReq));
            else 
            {
                craftData.Recipes[q].OutcomeId = listItems[q].Id;
                craftData.Recipes[q].Sources = listReq;
            }
        }

        if(craftData == null)   
            craftData = new GAME.CraftData(listRecipe);
        //else                    craftData.Recipes = listRecipe;

        WriteFile(filePath, craftData);
    }

    void Evaluate()
    {
        // reset runtime flags.
        for(int k = 0; k < EnforceGroups.Count; ++k)
        {
            for(int q = 0; q < EnforceGroups[k].Upgradables.Count; ++q)
                ResetRuntimeValues(EnforceGroups[k].Upgradables[q]);
        }
        for(int k = 1; k < EnforceGroups.Count; ++k)
            Evaluate(EnforceGroups[k]);
    }

    bool IsIdDuplicated(TimeUpgradableGroup.Type type, string id)
    {
        TimeUpgradableGroup group = EnforceGroups[(int)type];

        int cnt = 0;
        for(int q = 0; q < group.Upgradables.Count; ++q)
        {
            if(group.Upgradables[q].Id == id)
                ++cnt;
        }
        return cnt>=2;
    }

    void Evaluate(TimeUpgradableGroup group)
    {
        for(int q = 0; q < group.Upgradables.Count; ++q)
        {
            Evaluate(group.Upgradables[q]);
        }
    }

    void ResetRuntimeValues(TimeUpgradable TimeUpgradable)
    {
        TimeUpgradable.IsUsed = false;
    }

    void Evaluate(TimeUpgradable TimeUpgradable)
    {
        TimeUpgradable.ObtainTime = GetObtainSec(TimeUpgradable.Id);
    }
    float GetObtainSec(string id, bool isSource=false)
    {
        TimeUpgradable TimeUpgradable = GetTimeUpgradableById(id);
        if(TimeUpgradable == null)
            return -1.0f;

        //TimeUpgradable.IsUsed = isSource ? true : false;

        if(TimeUpgradable.Sources.Count <= 0)
            return TimeUpgradable.ObtainTime;

        float creationTime = .0f;
        for(int q = 0; q < TimeUpgradable.Sources.Count; ++q)
        {
            creationTime = Mathf.Max(creationTime, GetObtainSec(TimeUpgradable.Sources[q].SrcId, isSource:true) * ((float)TimeUpgradable.Sources[q].Count));
        }
        return creationTime > 0 ? creationTime : TimeUpgradable.ObtainTime;
    }

    bool IsResourceUsed(TimeUpgradableGroup.Type type, string id)
    {
        if(type >= TimeUpgradableGroup.Type.PROJECT)
            return true;

        for(int q = (int)type+1; q < (int)TimeUpgradableGroup.Type.MAX; ++q)
        {
            if(EnforceGroups[q].Upgradables.Count <= 0)
                continue;

            for(int k = 0; k < EnforceGroups[q].Upgradables.Count; ++k)
            {
                for(int s = 0; s < EnforceGroups[q].Upgradables[k].Sources.Count; ++s)
                {
                    if(id == EnforceGroups[q].Upgradables[k].Sources[s].SrcId)
                        return true;
                }
            }
        }
        return false;
    }

    TimeUpgradable GetTimeUpgradableById(string id)
    {
        for(int q = 0; q < EnforceGroups.Count; ++q)
        {
            var ret = EnforceGroups[q].GetUpgradable(id);
            if(ret != null)
                return ret;
        }
        return null;
    }

    string SecToTime(long sec)
    {
        return Core.Utils.TimeExt.ToTimeString(sec, Core.Utils.TimeExt.UnitOption.SHORT_NAME);
    }

    bool WriteFile(string filePath, object data)
    { 
        StreamWriter streamWriter = new StreamWriter(filePath);
        if(streamWriter == null)
            return false;

        string jsonStr = JsonUtility.ToJson(data, prettyPrint: true);
        streamWriter.Write(jsonStr);
        streamWriter.Close();
        streamWriter.Dispose();

        Debug.Log(jsonStr);
        return true;
    }

    private void CollectReferenceId(string materialId)
    {
        if (string.IsNullOrEmpty(materialId))
            return;

        RefIdList.Clear();
        RefIdList.Add(new List<string> { materialId });
        for(int g = 1; g < (int)TimeUpgradableGroup.Type.MAX; ++g)
        {
            var idList = new List<string>();

            TimeUpgradableGroup enfGroup = EnforceGroups[g];
            for(int i = 0; i < enfGroup.Upgradables.Count; ++i)
            {
                bool found = false;
                TimeUpgradable enfItem = enfGroup.Upgradables[i];
                for (int s = 0; s < enfItem.Sources.Count; ++s)
                {
                    // look up if referenced. 
                    int idxLast = RefIdList.Count - 1;
                    for (int id = 0; id < RefIdList[idxLast].Count; ++id)
                    {
                        if (RefIdList[idxLast][id] == enfItem.Sources[s].SrcId)
                        {
                            idList.Add(enfItem.Id);
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }

            RefIdList.Add(idList);
        }
    }

    bool IsReferencedId(int idxGroup, string itemId)
    {
        if(RefIdList.Count == 0) return false;
        if(idxGroup >= RefIdList.Count) 
            return false;   

        for(int q = 0; q < RefIdList[idxGroup].Count; ++q)
        {
            if (RefIdList[idxGroup][q] == itemId)
                return true;
        }
        return false;
    }

    async void TriggerActionWithDelayAsync(int milliSec, Action action)
    {
        await Task.Delay(milliSec);

        action?.Invoke();
    }
}
