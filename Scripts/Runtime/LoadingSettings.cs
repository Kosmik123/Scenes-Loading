using UnityEngine;

namespace Bipolar.SceneManagement
{
    [System.Serializable]
    public sealed class GlobalSceneContext
    {
        [SerializeField]
        [Tooltip("If false: init scene will be always loaded. If true: global context scenes will be always loaded")]
        private bool useGlobalScenesContext;
        public bool UseGlobalScenesContext => useGlobalScenesContext;

        [SerializeField]
        private SceneContext context;
        public SceneContext Context => context;
    }

    [CreateAssetMenu(menuName = Paths.Root + AssetName, fileName = AssetName)]
    public sealed class LoadingSettings : ScriptableObject
    {
        public const string AssetName = "Loading Settings";

        [Space]
        [SerializeField]
        [Tooltip("If not set: default loading strategy will be used.\nIf set: specified loading strategy will be used. ")]
        private Optional<LoadingStrategy> loadingStrategy;
        public LoadingStrategy LoadingStrategy => loadingStrategy;

        [SerializeField]
        [Tooltip("If not set: init scene will be always loaded.\nIf set: global context scenes will be always loaded.")]
        private Optional<SceneContext> globalScenesContext;
        public SceneContext GlobalScenesContext => globalScenesContext; // TODO: implement

        [SerializeField]
        [Tooltip("If not set: no context will be loaded on game start.\nIf set: starting context will be loaded on game start.")]
        private Optional<SceneContext> startingScenesContext;

        public SceneContext StartingScenesContext
        {
            get => startingScenesContext;
#if UNITY_EDITOR
            set => startingScenesContext = value;
#endif
        }

        private static LoadingSettings instance;
        public static LoadingSettings Instance
        {
            get
            {
                EnsureSettingsExist();
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateLoadingSettingsInstance()
        {
            if (Application.isPlaying == false)
                return;

            EnsureSettingsExist();
        }

        private static void EnsureSettingsExist()
        {
            if (instance == null)
            {
                Debug.Log("LoadingSetting missing. Loading from Resources");
                var settingsRequest = Resources.LoadAsync<LoadingSettings>(AssetName);
                if (settingsRequest != null)
                {
                    settingsRequest.completed += operation =>
                    {
                        if (settingsRequest.asset is LoadingSettings settings)
                        {
                            instance = settings;
                        }
                    };
                }
                else
                {
                    Debug.Log("No LoadingSettings in Resources. Creating empty settings");
                    instance = CreateInstance<LoadingSettings>();
                    instance.name = AssetName;
                }
            }
        }

        private void Reset()
        {
#if UNITY_EDITOR
            var loadingStrategiesGuids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(SceneManagement.LoadingStrategy)}");
            if (loadingStrategiesGuids != null && loadingStrategiesGuids.Length > 0)
            {
                var defaultStrategyPath = UnityEditor.AssetDatabase.GUIDToAssetPath(loadingStrategiesGuids[0]);
                loadingStrategy = UnityEditor.AssetDatabase.LoadAssetAtPath<LoadingStrategy>(defaultStrategyPath);
            }
#endif
        }

        private void OnEnable()
        {
            if (Instance == null)
            {
                Debug.Log("LoadingSettings loaded");
                instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (name != AssetName)
            {
                name = AssetName;
                UnityEditor.AssetDatabase.RenameAsset(UnityEditor.AssetDatabase.GetAssetPath(this), AssetName);
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }

    [System.Serializable]
    public struct Optional<T>
        where T : class
    {
        [SerializeField]
        private bool use;

        [SerializeField]
        private T _value;
        public T Value
        {
            readonly get => use ? _value : default;
            set
            {
                _value = value;
                use = value != null;
            }
        }

        public static implicit operator T(Optional<T> optional) => optional.Value;
        public static implicit operator Optional<T>(T optionalValue) => new Optional<T> { Value = optionalValue };
    }
}
