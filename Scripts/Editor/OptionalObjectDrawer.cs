using UnityEditor;
using UnityEngine;

namespace Bipolar.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalObjectDrawer : PropertyDrawer
    {
        private const int toggleWidth = 18;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            var useProperty = property.FindPropertyRelative("use");
            var valueProperty = property.FindPropertyRelative("value");

            float labelWidth = EditorGUIUtility.labelWidth;
            var labelRect = rect;
            labelRect.width = labelWidth;
            EditorGUI.LabelField(labelRect, label);


            var useRect = rect;
            useRect.x += labelWidth + 2;
            useRect.width = toggleWidth;
            EditorGUI.BeginChangeCheck();
            bool valueIsUsed = EditorGUI.Toggle(useRect, useProperty.boolValue);
            useProperty.boolValue = valueIsUsed;
            bool hasChanged = EditorGUI.EndChangeCheck();
            
            if (valueIsUsed)
            {
                var valueRect = rect;
                float valueRectX = labelWidth + 2 + toggleWidth + 2;
                valueRect.x += valueRectX;
                valueRect.height -= 2;
                valueRect.y += 1;
                valueRect.width -= valueRectX;

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
                hasChanged |= EditorGUI.EndChangeCheck();
            }

            if (hasChanged)
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }
    }
}
