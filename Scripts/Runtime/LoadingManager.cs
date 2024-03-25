using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bipolar.SceneManagement
{
    public delegate void ContextLoadingEventHandler(ScenesContext context);
    
    public class LoadingManager : MonoBehaviour
    {
        public static event System.Action OnLoadingStarted;
        public static event System.Action OnLoadingEnded;
        public static event System.Action<float> OnLoadingProgressChanged;

        internal static event System.Action OnInstanceCreated;

        public static LoadingManager Instance { get; private set; }
        public LoadingManagerSettings Settings { get; private set; }

        [SerializeField]
        private bool isLoading;
        public bool IsLoading => isLoading;

        [SerializeField]
        private float progress;
        public static float Progress => Instance == null ? 0 : Instance.progress;

        private ScenesContext currentContext;
        public ScenesContext CurrentContext => currentContext;

        private readonly List<AsyncOperation> sceneLoadOperations = new List<AsyncOperation>();
        private readonly List<IInitializer> initializers = new List<IInitializer>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
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
                progress = 0;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (IsInitSceneLoaded() == false)
            {
                SceneManager.LoadScene(0, LoadSceneMode.Additive);
            }
            else if (SceneManager.sceneCount == 1) // starting from InitScene
            {
                if (Settings && Settings.InitialScenesContext)
                {
                    LoadContext(Settings.InitialScenesContext);
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

        public void LoadContext(ScenesContext context) => Instance.LoadContextInternal(context);

        private void LoadContextInternal(ScenesContext context)
        {
            if (isLoading)
            {
                Debug.LogError("Loading Manager is already loading a context!");
                return;
            }

            progress = 0;
            isLoading = true;

            var scenesToUnload = new List<Scene>();
            var scenesToLoadIndices = new List<int>();
            UnloadUnneededScenes(context, scenesToUnload, scenesToLoadIndices);
            
            currentContext = context;
            OnLoadingStarted?.Invoke();

            sceneLoadOperations.Clear();
            for (int i = 0; i < scenesToLoadIndices.Count; i++)
            {
                int scene = scenesToLoadIndices[i];
                var loadingOperation = SceneManager.LoadSceneAsync(scene, new LoadSceneParameters(LoadSceneMode.Additive));
                sceneLoadOperations.Add(loadingOperation);
            }

            for (int i = 0; i < scenesToUnload.Count; i++)
            {
                Scene scene = scenesToUnload[i];
                var unloadingOperation = SceneManager.UnloadSceneAsync(scene);
                sceneLoadOperations.Add(unloadingOperation);
            }

            int activeSceneIndex = scenesToLoadIndices.Count == 0 ? 0 : scenesToLoadIndices[0];
            StartCoroutine(LoadingProcessCo(activeSceneIndex));
        }

        private void UnloadUnneededScenes(ScenesContext newContext, List<Scene> scenesToUnload, List<int> scenesToLoadIndices)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
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
            // LOADING SCENES FROM DISK
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
            Resources.UnloadUnusedAssets();

            // SCENES INITIALIZATION
            isDone = false;
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

                // float totalProgress = (scenesLoadProgress + initializationProgress) / 2;
                progress = 0.5f + initializationProgress / 2;
                OnLoadingProgressChanged?.Invoke(progress);
            }

            initializers.Clear();
            progress = 1;
            OnLoadingProgressChanged?.Invoke(progress);
            
            isLoading = false;
            OnLoadingEnded?.Invoke();
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
}
