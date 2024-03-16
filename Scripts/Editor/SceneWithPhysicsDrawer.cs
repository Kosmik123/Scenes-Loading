using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bipolar.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(SceneWithPhysics))]
    public class SceneWithPhysicsDrawer : PropertyDrawer
    {
        private const string scenePropertyName = "Scene";
        private readonly float scenePropertyHeight = EditorGUIUtility.singleLineHeight;
        private readonly float physicsPropertyHeight = EditorGUIUtility.singleLineHeight;
        private const float warningHelpboxHeight = 38;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded == false)
                return height;

            height += scenePropertyHeight + EditorGUIUtility.standardVerticalSpacing;
            var sceneProperty = property.FindPropertyRelative(scenePropertyName);
            var sceneAsset = sceneProperty.objectReferenceValue as SceneAsset;

            if (sceneAsset == null || GetSceneIndex(sceneAsset) <= 0) 
                height += warningHelpboxHeight + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            var sceneProperty = property.FindPropertyRelative(scenePropertyName);
            var sceneAsset = sceneProperty.objectReferenceValue as SceneAsset;
            string title = sceneAsset != null ? sceneAsset.name : "No scene"; 

            rect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, title);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                rect.height = scenePropertyHeight;
                EditorGUI.ObjectField(rect, sceneProperty);
                rect.y += scenePropertyHeight + EditorGUIUtility.standardVerticalSpacing;

                if (sceneAsset == null)
                {
                    rect.height = warningHelpboxHeight;
                    EditorGUI.HelpBox(rect, "Scene is missing!", MessageType.Error);
                    rect.y += warningHelpboxHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                else 
                { 
                    int sceneIndex = GetSceneIndex(sceneAsset);
                    if (sceneIndex <= 0)
                    {
                        rect.height = warningHelpboxHeight;
                        string message = sceneIndex < 0
                            ? $"Scene {sceneAsset.name} is not added in Build Settings"
                            : "Scene index is zero";

                        EditorGUI.HelpBox(rect, message, MessageType.Error);
                        rect.y += warningHelpboxHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }

                rect.height = physicsPropertyHeight;
                var physicsProperty = property.FindPropertyRelative("LocalPhysicsMode");
                if (physicsProperty != null) 
                    EditorGUI.PropertyField(rect, physicsProperty);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public static int GetSceneIndex(SceneAsset scene)
        {
            string fullPath = AssetDatabase.GetAssetPath(scene);
            int index = SceneUtility.GetBuildIndexByScenePath(fullPath);
            return index;
        }
    }
}
