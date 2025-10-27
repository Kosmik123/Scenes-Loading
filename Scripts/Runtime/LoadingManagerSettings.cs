using UnityEngine;

namespace Bipolar.SceneManagement
{
    [System.Serializable]
    public sealed class GlobalScenesContext
    {
        [SerializeField]
        [Tooltip("If false: init scene will be always loaded. If true: global context scenes will be always loaded")]
        private bool useGlobalScenesContext;
        public bool UseGlobalScenesContext => useGlobalScenesContext;

        [SerializeField]
        private ScenesContext context;
        public ScenesContext Context => context;
    }

    [CreateAssetMenu(menuName = Paths.Root + AssetName, fileName = AssetName)]
    public sealed class LoadingManagerSettings : ScriptableObject
    {
        public const string AssetName = "Loading Manager Settings";

        [Space]
        [SerializeField]
        [Tooltip("If not set: default loading strategy will be used.\nIf set: specified loading strategy will be used. ")]
        private Optional<LoadingStrategy> loadingStrategy;
        public LoadingStrategy LoadingStrategy => loadingStrategy;

        [SerializeField]
        [Tooltip("If not set: init scene will be always loaded.\nIf set: global context scenes will be always loaded.")]
        private Optional<ScenesContext> globalScenesContext;
        public ScenesContext GlobalScenesContext => globalScenesContext; // TODO: implement

        [SerializeField]
        [Tooltip("If not set: no context will be loaded on game start.\nIf set: starting context will be loaded on game start.")]
        private Optional<ScenesContext> startingScenesContext;
        public ScenesContext StartingScenesContext => startingScenesContext;

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
    public class Optional<T>
        where T : class
    {
        [SerializeField]
        private bool use;

        [SerializeField]
        private T _value;
        public T Value
        {
            get => use ? _value : default;
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
