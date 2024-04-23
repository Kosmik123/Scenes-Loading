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

    [CreateAssetMenu(menuName = CreateAssetPath.Root + AssetName, fileName = AssetName)]
    public sealed class LoadingManagerSettings : ScriptableObject
    {
        public const string AssetName = "Loading Manager Settings";

        [Space, SerializeField]
        private ScenesContext initialScenesContext;
        public ScenesContext InitialScenesContext => initialScenesContext;

        [SerializeField]
        private LoadingStrategy loadingStrategy;
        public LoadingStrategy LoadingStrategy => loadingStrategy;  

        [Space]
        [SerializeField]
        private GlobalScenesContext globalScenesContext;

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
    }
}
