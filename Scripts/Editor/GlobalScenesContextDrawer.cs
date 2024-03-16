using UnityEditor;
using UnityEngine;

namespace Bipolar.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(GlobalScenesContext))]
    public class GlobalScenesContextDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var useGlobalScenesContextProperty = property.FindPropertyRelative("useGlobalScenesContext");
            bool isUsingGlobalContext = useGlobalScenesContextProperty.boolValue;
            if (isUsingGlobalContext)
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            var useGlobalScenesContextProperty = property.FindPropertyRelative("useGlobalScenesContext");
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, useGlobalScenesContextProperty);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            bool isUsingGlobalContext = useGlobalScenesContextProperty.boolValue;
            if (isUsingGlobalContext)
            {
                EditorGUI.indentLevel++;
                var contextProperty = property.FindPropertyRelative("context");
                EditorGUI.PropertyField(rect, contextProperty);
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; 
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
