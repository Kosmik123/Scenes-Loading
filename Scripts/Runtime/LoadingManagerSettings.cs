using UnityEngine;

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

    [CreateAssetMenu(menuName = CreateAssetPath.Root + "Loading Manager Settings", fileName = AssetName)]
    public class LoadingManagerSettings : ScriptableObject
    {
        public const string AssetName = "Loading Manager Settings";

        [Space, SerializeField]
        private ScenesContext initialScenesContext;
        public ScenesContext InitialScenesContext => initialScenesContext;

        [Space, SerializeField]
        private GlobalScenesContext globalScenesContext;
    }
}
