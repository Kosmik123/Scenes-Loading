using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Bipolar.SceneManagement.Editor
{
    internal sealed class ScenesContextsBeforeBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => default;

        public void OnPreprocessBuild(BuildReport report)
        {
            string filter = $"t:{typeof(ScenesContext).Name}";
            var allContextsGuids = AssetDatabase.FindAssets(filter);
            foreach (var guid in allContextsGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scenesContext = AssetDatabase.LoadAssetAtPath<ScenesContext>(path);
                if (scenesContext)
                    scenesContext.SerializeScenesIndices();
            }
        }
    }
}
