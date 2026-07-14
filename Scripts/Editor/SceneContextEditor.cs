using UnityEditor;

namespace Bipolar.SceneManagement.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(SceneContext))]
    public class SceneContextEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            bool isStartingContext = LoadingSettings.Instance.StartingScenesContext == target;
            bool isSet = EditorGUILayout.Toggle("Is Starting Context", isStartingContext);
            if (isSet != isStartingContext)
            {
                LoadingSettings.Instance.StartingScenesContext = (SceneContext)target;
                EditorUtility.SetDirty(LoadingSettings.Instance);   
            }

            base.OnInspectorGUI();
        }
    }
}
