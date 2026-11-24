using UnityEditor;
using UnityEngine;

namespace Bipolar.SceneManagement.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(SceneContext))]
    public class SceneContextEditor : Editor
    {
        private int playModeScenesCount;
        private int totalScenesCount;

        private void OnEnable()
        {
            RefreshPlayModeUsage();
        }

        public override void OnInspectorGUI()
        {
            if (LoadingSettings.Instance)
            {
                bool isStartingContext = LoadingSettings.Instance.StartingScenesContext == target;
                bool isSet = EditorGUILayout.Toggle("Is Starting Context", isStartingContext);
                if (isSet != isStartingContext)
                {
                    LoadingSettings.Instance.StartingScenesContext = (SceneContext)target;
                    EditorUtility.SetDirty(LoadingSettings.Instance);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("LoadingSettings asset is missing", MessageType.Error);
            }

            base.OnInspectorGUI();
            DrawPlayModeSection();
        }

        private void DrawPlayModeSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Playing from a scene", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"This context is loaded when pressing Play in {playModeScenesCount} of its {totalScenesCount} scenes.",
                MessageType.None);

            if (GUILayout.Button("Use when playing from its scenes"))
            {
                PlayModeContextSelector.UseContextForItsScenes((SceneContext)target);
                RefreshPlayModeUsage();
            }
        }

        private void RefreshPlayModeUsage()
        {
            playModeScenesCount = PlayModeContextSelector.CountScenesUsingContext((SceneContext)target, out totalScenesCount);
        }
    }
}
