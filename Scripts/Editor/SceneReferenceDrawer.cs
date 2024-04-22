using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bipolar.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        private const string scenePropertyName = "Scene";
        private readonly float scenePropertyHeight = EditorGUIUtility.singleLineHeight;
        private readonly float physicsPropertyHeight = EditorGUIUtility.singleLineHeight;
        private const float warningHelpboxHeight = 38;
        private readonly float addSceneToBuildSettingsButtonHeight = 20;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + 1;
            if (property.isExpanded == false)
                return height;

            height += scenePropertyHeight + EditorGUIUtility.standardVerticalSpacing;
            var sceneProperty = property.FindPropertyRelative(scenePropertyName);
            var sceneAsset = sceneProperty.objectReferenceValue as SceneAsset;

            if (sceneAsset == null)
            {
                height += warningHelpboxHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                int sceneIndex = GetSceneIndex(sceneAsset);
                if (sceneIndex <= 0)
                {
                    height += warningHelpboxHeight + EditorGUIUtility.standardVerticalSpacing;
                    if (sceneIndex < 0)    
                        height += addSceneToBuildSettingsButtonHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

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
                    EditorGUI.HelpBox(rect, "Scene is missing", MessageType.Error);
                    rect.y += warningHelpboxHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                else 
                { 
                    int sceneIndex = GetSceneIndex(sceneAsset);
                    if (sceneIndex < 0)
                    {
                        rect.height = warningHelpboxHeight;
                        string message = $"Scene {sceneAsset.name} is not added in Build Settings";
     
                        EditorGUI.HelpBox(rect, message, MessageType.Error);
                        rect.y += warningHelpboxHeight + EditorGUIUtility.standardVerticalSpacing;
                        rect.height = addSceneToBuildSettingsButtonHeight;
                        if (GUI.Button(rect, "Add scene to Build Settings"))
                        {
                            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                            int oldScenesCount = EditorBuildSettings.scenes.Length;
                            var newScenes = new EditorBuildSettingsScene[oldScenesCount + 1];
                            System.Array.Copy(EditorBuildSettings.scenes, newScenes, oldScenesCount);
                            newScenes[oldScenesCount] = new EditorBuildSettingsScene(scenePath, true);
                            EditorBuildSettings.scenes = newScenes;
                        }
                    }
                    else if (sceneIndex == 0)
                    {
                        rect.height = warningHelpboxHeight;
                        string message = "Scene Build Index = 0. Index 0 is reserved for initial scene which cannot be contained in contexts";
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
