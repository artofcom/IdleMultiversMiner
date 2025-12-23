using UnityEditor;
using UnityEngine;

// ReadOnlyAttribute가 적용된 모든 프로퍼티를 대상으로 합니다.
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    // GUI를 그리는 메서드 재정의
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // GUI를 비활성화합니다. 이렇게 하면 필드가 Inspector에 표시되지만 편집할 수 없습니다.
        GUI.enabled = false;

        // 실제 필드를 그립니다.
        EditorGUI.PropertyField(position, property, label, true);

        // GUI를 다시 활성화하여 다른 필드에 영향을 주지 않도록 합니다.
        GUI.enabled = true;
    }

    // 필드의 높이를 반환합니다 (기본 필드와 동일).
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}