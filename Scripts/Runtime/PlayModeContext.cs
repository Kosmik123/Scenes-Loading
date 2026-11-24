#if UNITY_EDITOR
using UnityEditor;

namespace Bipolar.SceneManagement
{
    /// <summary>
    /// Editor only. Holds the context that should be loaded after Boot scene when Play mode
    /// was started from a scene other than Boot. Set by the editor on entering Play mode.
    /// </summary>
    internal static class PlayModeContext
    {
        private const string SessionKey = "Bipolar.SceneManagement.PlayModeContext";

        public static SceneContext Get()
        {
            string guid = SessionState.GetString(SessionKey, string.Empty);
            if (string.IsNullOrEmpty(guid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<SceneContext>(path);
        }

        public static void Set(SceneContext context)
        {
            string guid = context ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(context)) : string.Empty;
            SessionState.SetString(SessionKey, guid);
        }
    }
}
#endif
