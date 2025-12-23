using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ImplementsInterfaceAttribute))]
public class InterfaceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ObjectReference)
        {
            EditorGUI.HelpBox(position, "ImplementsInterfaceAttribute works only on object reference fields.", MessageType.Error);
            return;
        }

        var attribute = (ImplementsInterfaceAttribute)this.attribute;
        System.Type targetType = attribute.TargetType;

        EditorGUI.BeginProperty(position, label, property);
        
        Object currentObject = property.objectReferenceValue;
        Object newObject = EditorGUI.ObjectField(position, label, currentObject, typeof(UnityEngine.Object), true);

        if (newObject != currentObject)
        {
            if (newObject == null)
            {
                property.objectReferenceValue = null;
            }
            else
            {
                Object implementingObject = null;
                if (targetType.IsInstanceOfType(newObject))
                {
                    implementingObject = newObject;
                }
                else
                {
                    GameObject newGameObject = newObject as GameObject;
                    if (newGameObject != null)
                    {
                        implementingObject = newGameObject.GetComponents<Component>()
                                            .OfType<MonoBehaviour>()
                                            .FirstOrDefault(c => targetType.IsInstanceOfType(c));
                    }
                }

                if (implementingObject != null)
                {
                    property.objectReferenceValue = implementingObject;
                }
                else
                {
                    Debug.LogWarning($"The object '{newObject.name}' does not implement interface '{targetType.Name}' or is not a valid Unity object.");
                    property.objectReferenceValue = currentObject; 
                }
            }
        }
        
        EditorGUI.EndProperty();
    }
}