using UnityEditor;

namespace Bipolar.SceneManagement.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(SceneContext))]
    public class SceneContextEditor : Editor
    {
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
        }
    }
}
