using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Bipolar.SceneManagement.Editor
{
    internal sealed class SceneContextsBeforeBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => default;

        public void OnPreprocessBuild(BuildReport report)
        {
            string filter = $"t:{typeof(SceneContext).Name}";
            var allContextsGuids = AssetDatabase.FindAssets(filter);
            foreach (var guid in allContextsGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scenesContext = AssetDatabase.LoadAssetAtPath<SceneContext>(path);
                if (scenesContext)
                    scenesContext.SerializeScenesIndices();
            }
        }
    }
}
