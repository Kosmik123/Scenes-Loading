using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bipolar.SceneManagement.Core;
using NaughtyAttributes;

namespace Bipolar.SceneManagement
{
    public delegate void ContextLoadingEventHandler(ScenesContext context);

    public sealed class LoadingManager : MonoBehaviour
    {
        public static event System.Action OnLoadingStarted;
        public static event System.Action OnLoadingEnded;
        public static event System.Action<float> OnLoadingProgressChanged;

        internal static event System.Action OnInstanceCreated;

        public static LoadingManager Instance { get; private set; }
        public LoadingManagerSettings Settings { get; private set; }

        [SerializeField, ReadOnly]
        private bool isLoading;
        public bool IsLoading => isLoading;

        [SerializeField, ReadOnly]
        private float progress;
        public static float Progress => Instance == null ? 0 : Instance.progress;

        [SerializeField, ReadOnly]
        private ScenesContext currentContext;
        public ScenesContext CurrentContext => currentContext;

        [SerializeField, ReadOnly]
        private List<Scene> currentlyLoadedScenes = new List<Scene>();

        // should this be in a different class?
        private readonly List<AsyncOperation> sceneLoadOperations = new List<AsyncOperation>();
        private readonly List<IInitializer> initializers = new List<IInitializer>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateLoadingManager()
        {
            var settingsRequest = Resources.LoadAsync(LoadingManagerSettings.AssetName);
            settingsRequest.completed += operation =>
            {
                var loadingManager = new GameObject().AddComponent<LoadingManager>();
                loadingManager.name = "Loading Manager";
                if (settingsRequest.asset is LoadingManagerSettings settings)
                {
                    loadingManager.Settings = settings; 
                }
                Instance = loadingManager;
                OnInstanceCreated?.Invoke();
            };
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                isLoading = false;
                progress = 1;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                    currentlyLoadedScenes.Add(scene);
            }
        }

        private void Start()
        {
            if (IsInitSceneLoaded() == false)
            {
                SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive).completed += operation =>
                {
                    currentlyLoadedScenes.Add(SceneManager.GetSceneAt(SceneManager.loadedSceneCount - 1));
                };
            }
            else if (SceneManager.sceneCount == 1) // starting from InitScene
            {
                if (Settings && Settings.StartingScenesContext)
                {
                    LoadContext(Settings.StartingScenesContext, forced: true);
                }
                else
                {
                    isLoading = false;
                    progress = 1;
                    OnLoadingEnded?.Invoke();
                }
            }
            OnInstanceCreated = null;
        }

        public bool IsInitSceneLoaded()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).buildIndex == 0)
                    return true;

            return false;
        }

        public bool LoadContext(ScenesContext context, LoadingStrategy loadingStrategy = null, bool forced = false)
        {
            if (forced == false && currentContext == context)
                return false;

            return Instance.LoadContextInternal(context, loadingStrategy);
        }

        private bool LoadContextInternal(ScenesContext context, LoadingStrategy loadingStrategy)
        {
            if (loadingStrategy == null && Settings)
                loadingStrategy = Settings.LoadingStrategy;
            if (loadingStrategy == null)
                loadingStrategy = DefaultLoadingStrategy.Instance;

            if (isLoading)
            {
                Debug.LogError("Loading Manager is already loading a context!");
                return false;
            }

            isLoading = true;
            progress = 0;

            OnLoadingStarted?.Invoke();
            currentContext = context;

            var scenesToUnload = new List<Scene>();
            var scenesToLoadIndices = new List<int>();
            GetScenesToUnload(context, scenesToUnload, scenesToLoadIndices);

            sceneLoadOperations.Clear();
            for (int i = 0; i < scenesToLoadIndices.Count; i++)
            {
                int loadedSceneIndex = scenesToLoadIndices[i];
                var loadingOperation = SceneManager.LoadSceneAsync(loadedSceneIndex, new LoadSceneParameters(LoadSceneMode.Additive));
                sceneLoadOperations.Add(loadingOperation);
                loadingOperation.completed += operation =>
                {
                    currentlyLoadedScenes.Add(SceneManager.GetSceneByBuildIndex(loadedSceneIndex));
                };
            }

            for (int i = 0; i < scenesToUnload.Count; i++)
            {
                var scene = scenesToUnload[i];
                var unloadingOperation = SceneManager.UnloadSceneAsync(scene);
                sceneLoadOperations.Add(unloadingOperation);
                unloadingOperation.completed += operation => 
                {
                    currentlyLoadedScenes.Remove(scene);
                };
            }

            int activeSceneIndex = scenesToLoadIndices.Count == 0 ? 0 : currentContext.Scenes[0].BuildIndex;
            StartCoroutine(LoadingProcessCo(activeSceneIndex));

            return true;
        }

        private IEnumerator LoadContextUsingStrategy(ScenesContext context, LoadingStrategy strategy)
        {
            progress = 0;
            isLoading = true;
            OnLoadingStarted?.Invoke();

            currentContext = context;

            foreach (var progress in strategy.LoadContext(context))
            {
                this.progress = progress;
                yield return null;
            }

            progress = 1;
            isLoading = false;
            OnLoadingEnded?.Invoke();
        }

        private void GetScenesToUnload(ScenesContext newContext, List<Scene> scenesToUnload, List<int> scenesToLoadIndices)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                int buildIndex = scene.buildIndex;
                if (buildIndex > 0)
                    scenesToUnload.Add(scene);
            }

            for (int i = 0; i < newContext.Scenes.Count; i++)
            {
                var scene = newContext.Scenes[i];
                int index = scenesToUnload.FindIndex(s => s.buildIndex == scene.BuildIndex);
                if (index >= 0)
                {
                    scenesToUnload.RemoveAt(index);
                }
                else
                {
                    scenesToLoadIndices.Add(scene.BuildIndex);
                }
            }
        }

        private IEnumerator LoadingProcessCo(int activeSceneIndex)
        {
            yield return LoadScenesFromDisk();
            Resources.UnloadUnusedAssets();
            yield return InitializeScenes();

            initializers.Clear();
            progress = 1;
            OnLoadingProgressChanged?.Invoke(progress);

            for (int i = 0; i < currentlyLoadedScenes.Count; i++)
            {
                if (currentlyLoadedScenes[i].buildIndex == activeSceneIndex)
                {
                    SceneManager.SetActiveScene(currentlyLoadedScenes[i]);
                    break;
                }
            }

            isLoading = false;
            OnLoadingEnded?.Invoke();
        }

        private IEnumerable LoadScenesFromDisk()
        {
            bool isDone = false;
            while (isDone == false)
            {
                yield return null;
                isDone = true;

                float scenesLoadProgress = 0;
                int scenesCount = sceneLoadOperations.Count;
                for (int i = 0; i < scenesCount; i++)
                {
                    var operation = sceneLoadOperations[i];
                    scenesLoadProgress += operation.progress;
                    isDone = isDone && operation.isDone;
                }
                scenesLoadProgress /= scenesCount;
                progress = scenesLoadProgress / 2;
                OnLoadingProgressChanged?.Invoke(progress);
            }
        }

        private IEnumerable InitializeScenes()
        {
            bool isDone = false;
            while (isDone == false)
            {
                yield return null;
                isDone = true;

                int initializersCount = initializers.Count;
                float initializationProgress = 0;
                for (int i = 0; i < initializersCount; i++)
                {
                    var initializer = initializers[i];
                    initializationProgress += initializer.InitializationProgress;
                    isDone = isDone && initializer.IsInitialized;
                }
                initializationProgress /= initializersCount;

                progress = 0.5f + initializationProgress / 2;
                OnLoadingProgressChanged?.Invoke(progress);
            }
        }

        internal static void AddInitializer(IInitializer initializer)
        {
            if (Instance)
            {
                Instance.AddInitializerInternal(initializer);
            }
#if UNITY_EDITOR
            else
            {
                OnInstanceCreated += () => Instance.AddInitializerInternal(initializer);
            }
#endif
        }

        private void AddInitializerInternal(IInitializer initializer)
        {
            if (isLoading == false)
                return;

            initializers.Add(initializer);
        }

    }

    public static class SceneExtensions
    {
        public static string ToString(this Scene scene)
        {
            return $"Scene: {scene.buildIndex} : {scene.handle}";
        }
    }
}
