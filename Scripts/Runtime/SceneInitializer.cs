using UnityEngine;

namespace Bipolar.SceneManagement
{
    public abstract class SceneInitializer : MonoBehaviour, Core.IInitializer
    {
        public abstract float InitializationProgress { get; }
        public virtual bool IsInitialized => InitializationProgress >= 1;

        protected virtual void Awake()
        {
            LoadingManager.AddInitializer(this);
        }
    }
}
