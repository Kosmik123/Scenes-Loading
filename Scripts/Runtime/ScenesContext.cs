using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bipolar.SceneManagement
{
#if UNITY_EDITOR
    [System.Serializable]
    public class SceneReference
    {
        public UnityEditor.SceneAsset Scene;
        //public LocalPhysicsMode LocalPhysicsMode;
    }
#endif

    [System.Serializable]
    public struct SceneData
    {
        [field: SerializeField]
        public int BuildIndex { get; private set; }
        //[field: SerializeField]
        //public LocalPhysicsMode LocalPhysicsMode { get; private set; }

        public SceneData(int buildIndex, LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            BuildIndex = buildIndex;
            //LocalPhysicsMode = physicsMode;
        }
    }

    public static class CreateAssetPath
    {
        public const string Root = "Bipolar/Scene Management/";
    }

    [CreateAssetMenu(menuName = CreateAssetPath.Root + "Scenes Context", order = 2)]
    public class ScenesContext : ScriptableObject 
    {
#if UNITY_EDITOR
        [SerializeField]
        private SceneReference[] scenes;
#endif
        [SerializeField, HideInInspector]
        private SceneData[] scenesData;
        public IReadOnlyList<SceneData> Scenes
        {
            get
            {
#if UNITY_EDITOR
                if (scenesData == null || scenesData.Length != scenes.Length) 
                    SerializeScenesIndices();
#endif
                return scenesData;
            }
        }

#region Editor Code
#if UNITY_EDITOR
        [ContextMenu("Validate Scenes")]
        public void SerializeScenesIndices()
        {
            var scenesList = new List<SceneReference>(scenes);
            var sceneDatasList = new List<SceneData>();

            for (int i = scenesList.Count - 1; i >= 0; i--)
            {
                var data = scenesList[i];
                if (data.Scene == null)
                {
                    Debug.LogWarning("Scene cannot be null");
                    continue;
                }

                int buildIndex = GetSceneIndex(data.Scene);
                if (buildIndex == 0)
                {
                    Debug.LogWarning("Scene with index 0 is init scene and will be always loaded!");
                    continue;
                }
                else if (buildIndex < 0)
                {
                    Debug.LogWarning($"Scene {data.Scene.name} is not added in Build Settings");
                    continue;
                }

                sceneDatasList.Add(new SceneData(buildIndex/*, data.LocalPhysicsMode */));
            }

            sceneDatasList.Reverse();

            scenes = scenesList.ToArray();
            scenesData = sceneDatasList.ToArray();

            static int GetSceneIndex(UnityEditor.SceneAsset scene)
            {
                string fullPath = UnityEditor.AssetDatabase.GetAssetPath(scene);
                int index = SceneUtility.GetBuildIndexByScenePath(fullPath);
                return index;
            }
        }

        [ContextMenu("Load Context")]
        private void LoadContext()
        {
            if (Application.isPlaying)
            {
                LoadingManager.Instance.LoadContext(this);
            }
            else 
            {
                int loadedScenesCount = SceneManager.sceneCount;
                var scenesToUnload  = new List<Scene>();
                for (int i = 0; i < loadedScenesCount; i++)
                    scenesToUnload.Add(SceneManager.GetSceneAt(i));

                var scenesToLoadIndices  = new List<int>();
                int scenesToLoadCount = Scenes.Count;
                for (int i = 0; i < scenesToLoadCount; i++)
                {
                    var sceneToLoad = Scenes[i];
                    int sceneIndexInUnloaded = scenesToUnload.FindIndex(scene => scene.buildIndex == sceneToLoad.BuildIndex);
                    if (sceneIndexInUnloaded >= 0)
                    {
                        scenesToUnload.RemoveAt(sceneIndexInUnloaded);
                    }
                    else
                    {
                        scenesToLoadIndices.Add(sceneToLoad.BuildIndex);
                    }
                }

                foreach (var sceneIndex in scenesToLoadIndices)
                {
                    string path = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                }

                foreach (var scene in scenesToUnload)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                }
            }
        }
#endif
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (scenes != null)
            {
                SerializeScenesIndices();
            }
#endif
        }
#endregion

    }

    public static class ScenesContextExtensions
    {
        public static void LoadContext(this ScenesContext context)
        {
        }
    }


}
