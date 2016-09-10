using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(ThreadsafeCurve))]
public class ThreadsafeCurvePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_curve"), GUIContent.none);
        EditorGUI.EndProperty();
    }
}