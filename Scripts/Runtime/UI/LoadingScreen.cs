using UnityEngine;

namespace Bipolar.SceneManagement.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        protected virtual void OnLoadingStarted(ScenesContext context) { }
        protected virtual void OnLoadingProgressChanged(float progress) { }
        protected virtual void OnLoadingEnded(ScenesContext context) { }

        private void Awake()
        {
            LoadingManager.Instance.OnLoadingStarted += OnLoadingStarted;
            LoadingManager.Instance.OnLoadingProgressChanged += OnLoadingProgressChanged;
            LoadingManager.Instance.OnLoadingEnded += OnLoadingEnded;
        }

        private void OnDestroy()
        {
            LoadingManager.Instance.OnLoadingStarted -= OnLoadingStarted;
            LoadingManager.Instance.OnLoadingProgressChanged -= OnLoadingProgressChanged;
            LoadingManager.Instance.OnLoadingEnded -= OnLoadingEnded;
        }
    }
}
