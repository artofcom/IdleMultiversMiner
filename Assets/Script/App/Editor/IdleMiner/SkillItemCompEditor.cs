using App.GamePlay.IdleMiner.SkillTree;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillItemComp))]
public class SkillItemCompEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SkillItemComp comp = (SkillItemComp)target;
        comp.SetSkillId(comp.SkillData.Id);

        DrawDefaultInspector();     // 기본 인스펙터 표시
    }
}
