using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bipolar.SceneManagement
{
    [System.Serializable]
    public class GlobalScenesContext
    {
        [SerializeField]
        private bool useGlobalScenesContext;
        public bool UseGlobalScenesContext => useGlobalScenesContext;

        [SerializeField]
        private ScenesContext context;
        public ScenesContext Context => context;
    }

    public delegate void ContextLoadingEventHandler(ScenesContext context);

    public class LoadingManager : MonoBehaviour
    {
        private const string PrefabName = "Loading Manager";

        public event ContextLoadingEventHandler OnLoadingStarted;
        public event ContextLoadingEventHandler OnLoadingEnded;
        public event System.Action<float> OnLoadingProgressChanged;

        public static LoadingManager Instance { get; private set; }

        [SerializeField]
        private GlobalScenesContext globalScenesContext;

        [Space, SerializeField]
        private ScenesContext initialScenesContext;

        [SerializeField]
        private bool isLoading;
        public bool IsLoading => isLoading;

        [SerializeField]
        private float progress;
        public float Progress => progress;

        private ScenesContext currentContext;

        private readonly List<AsyncOperation> sceneLoadOperations = new List<AsyncOperation>();
        private readonly List<IInitializer> initializers = new List<IInitializer>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var loadingManagerRequest = Resources.LoadAsync(PrefabName);
            loadingManagerRequest.completed += operation =>
            {
                var prefab = loadingManagerRequest.asset;
                var loadingManager = Instantiate(prefab);
                loadingManager.name = PrefabName;
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
                if (initialScenesContext)
                {
                    LoadContext(initialScenesContext);
                }
            }
        }

        public bool IsInitSceneLoaded()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).buildIndex == 0)
                    return true;

            return false;
        }

        public static void LoadContext(ScenesContext context) => Instance.LoadContextInternal(context);

        private void LoadContextInternal(ScenesContext context)
        {
            if (isLoading)
            {
                Debug.LogError("Loading Manager is already loading a context!");
                return;
            }

            var scenesToUnload = new List<Scene>();
            var scenesToLoadIndices = new List<int>();
            UnloadUnneededScenes(context, scenesToUnload, scenesToLoadIndices);


            OnLoadingStarted?.Invoke(context);

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

        private void UnloadUnneededScenes(ScenesContext context, List<Scene> scenesToUnload, List<int> scenesToLoadIndices)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                int buildIndex = scene.buildIndex;
                if (buildIndex > 0)
                    scenesToUnload.Add(scene);
            }

            for (int i = 0; i < context.Scenes.Count; i++)
            {
                var scene = context.Scenes[i];
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

            sceneLoadOperations.Clear();
            progress = 0;
            isLoading = true;
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
            }

            initializers.Clear();
            progress = 1;
            isLoading = false;
            OnLoadingEnded?.Invoke(currentContext);
        }

        internal static void AddInitializer(IInitializer initializer) => Instance.InternalAddInitializer(initializer);

        private void InternalAddInitializer(IInitializer initializer)
        {
            if (isLoading == false)
                return;

            initializers.Add(initializer);
        }
    }
}
