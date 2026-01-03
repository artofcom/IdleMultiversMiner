using App.GamePlay.IdleMiner;
using IGCore.PlatformService.Util;
using UnityEditor;
using UnityEngine;

public class StringDecompressEditor : EditorWindow
{
    private string inputString = "";
    private string resultString = "";
    Vector2 scrollPosition;

    [MenuItem("PlasticGames/Editor/StringDecompressor")]
    public static void ShowWindow()
    {
        GetWindow<StringDecompressEditor>().Init();
    }

    public void Init()
    {
    }
    
    public void OnGUI()
    {
        GUILayout.Label("String Decompressor.", EditorStyles.boldLabel);


        inputString = EditorGUILayout.TextField("Input Compressed String", inputString);

        GUILayout.Space(10);
        if (GUILayout.Button("Decompress"))
            ProcessText();
        


        EditorGUILayout.LabelField("Decompressed String:", EditorStyles.boldLabel);
        // EditorGUILayout.SelectableLabel(resultString, EditorStyles.helpBox);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        
        resultString = EditorGUILayout.TextArea(resultString, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();
    }

    private void ProcessText()
    {
        resultString = StringCompressor.DecompressFromEncodedString(inputString);
    }
    
}