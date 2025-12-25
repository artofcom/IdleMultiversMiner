using UnityEditor;
using UnityEngine;
using App.GamePlay.IdleMiner;

public class IdleMinerUnitEditorWindow : EditorWindow
{
    const int WIDTH = 1500;
    const int LANE_W = 500;
    IdleMinerUnit mainUnit;

    
    Vector2 vScrollPos = Vector2.zero;

    public static void ShowWindow(IdleMinerUnit minerUnit)
    {
        IdleMinerUnitEditorWindow window = GetWindow<IdleMinerUnitEditorWindow>("Main Simulation Window");

        window.position = new Rect(100, 100, WIDTH, 500);
        window.Init(minerUnit);
    }

    public void Init(IdleMinerUnit comp)
    {
        mainUnit = comp;
    }

    public void OnGUI()
    {
        vScrollPos = GUILayout.BeginScrollView(vScrollPos);
        {
            // Dummy and Tests.
            GUILayout.Space(10);
           // GUILayout.Label("=== Numbering for less confusing");
            GUILayout.Label("=== Missing link check");

            DrawSimpleGraph();
        }
        GUILayout.EndScrollView();
    }

    private void DrawSimpleGraph()
    {
        // 데이터 배열 (예: 5개의 값)
        int[] data = { 50, 80, 30, 90, 60 };
        float maxHeight = 100f; // 그래프의 최대 높이

        GUILayout.BeginHorizontal();
        GUILayout.Space(10); // 왼쪽 여백

        foreach (int value in data)
        {
            GUILayout.BeginVertical();

            // 1. 막대 그래프의 빈 공간 (남은 높이)
            float remainingHeight = maxHeight - value;
            EditorGUILayout.Space(remainingHeight); 

            // 2. 실제 막대 (데이터 값 높이)
            Rect barRect = EditorGUILayout.GetControlRect(false, value); // 높이를 값만큼 설정
        
            // 막대 색상 설정 및 그리기
            EditorGUI.DrawRect(barRect, Color.blue); 

            GUILayout.EndVertical();
            GUILayout.Space(5); // 막대 사이의 간격
        }

        GUILayout.EndHorizontal();
        // 창이 업데이트되도록 강제 요청
        Repaint(); 
    }
}
