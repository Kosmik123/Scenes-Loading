using UnityEngine;

namespace Bipolar.SceneManagement
{
    public interface IInitializer
    {
        float InitializationProgress { get; }
        bool IsInitialized { get; }
    }

    public abstract class SceneInitializer : MonoBehaviour, IInitializer
    {
        public abstract float InitializationProgress { get; }
        public virtual bool IsInitialized => InitializationProgress >= 1;

        protected virtual void Awake()
        {
            LoadingManager.AddInitializer(this);
        }
    }
}
