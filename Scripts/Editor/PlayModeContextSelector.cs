using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bipolar.SceneManagement.Editor
{
    /// <summary>
    /// Chooses which context is loaded when Play mode is started from a scene other than Boot.
    /// The scene-to-context mapping is derived from the contexts themselves.
    /// If the scene belongs to several contexts, the personal choice is used
    /// (see <see cref="PlayModeContextChoices"/>), otherwise the first one by asset path.
    /// </summary>
    [InitializeOnLoad]
    internal static class PlayModeContextSelector
    {
        static PlayModeContextSelector()
        {
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            var scenePath = SceneManager.GetActiveScene().path;
            var context = FindContextFor(scenePath, out int candidatesCount);
            PlayModeContext.Set(context);

            if (candidatesCount > 1)
                Debug.Log($"Scene '{scenePath}' belongs to {candidatesCount} contexts. Loading '{context.name}'. " +
                    "To change it, select another context and press 'Use when playing from its scenes'.", context);
        }

        /// <summary>
        /// Returns the context to load when playing from the scene, or null if there is none
        /// (the scene is Boot, is not saved, or does not belong to any context).
        /// </summary>
        internal static SceneContext FindContextFor(string scenePath, out int candidatesCount)
        {
            candidatesCount = 0;
            if (string.IsNullOrEmpty(scenePath))
                return null;

            if (SceneUtility.GetBuildIndexByScenePath(scenePath) == 0)
                return null;

            var candidates = FindContextsContaining(scenePath);
            candidatesCount = candidates.Count;
            if (candidatesCount == 0)
                return null;

            if (candidatesCount == 1)
                return candidates[0];

            string sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
            if (PlayModeContextChoices.instance.TryGetContextGuid(sceneGuid, out string contextGuid))
                foreach (var candidate in candidates)
                    if (GetGuid(candidate) == contextGuid)
                        return candidate;

            return candidates[0];
        }

        /// <summary>
        /// Counts the scenes of the context from which Play mode would load this context.
        /// </summary>
        internal static int CountScenesUsingContext(SceneContext context, out int totalScenesCount)
        {
            int usingCount = 0;
            totalScenesCount = 0;
            foreach (var sceneAsset in context.GetSceneAssets())
            {
                totalScenesCount++;
                if (FindContextFor(AssetDatabase.GetAssetPath(sceneAsset), out _) == context)
                    usingCount++;
            }
            return usingCount;
        }

        internal static void UseContextForItsScenes(SceneContext context)
        {
            string contextGuid = GetGuid(context);
            foreach (var sceneAsset in context.GetSceneAssets())
                PlayModeContextChoices.instance.SetChoice(GetGuid(sceneAsset), contextGuid);
        }

        private static List<SceneContext> FindContextsContaining(string scenePath)
        {
            var result = new List<SceneContext>();
            var guids = AssetDatabase.FindAssets($"t:{nameof(SceneContext)}");
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var context = AssetDatabase.LoadAssetAtPath<SceneContext>(path);
                if (context && ContainsScene(context, scenePath))
                    result.Add(context);
            }

            result.Sort((a, b) => string.CompareOrdinal(AssetDatabase.GetAssetPath(a), AssetDatabase.GetAssetPath(b)));
            return result;
        }

        private static bool ContainsScene(SceneContext context, string scenePath)
        {
            foreach (var sceneAsset in context.GetSceneAssets())
                if (AssetDatabase.GetAssetPath(sceneAsset) == scenePath)
                    return true;

            return false;
        }

        private static string GetGuid(Object asset)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
        }
    }
}
