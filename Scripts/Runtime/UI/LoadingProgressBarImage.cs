using UnityEngine;
using UnityEngine.UI;

namespace Bipolar.SceneManagement.UI
{
    public class LoadingProgressBarImage : MonoBehaviour
    {
        [SerializeField]
        private Image progressBar;

        private void OnEnable()
        {
            progressBar.fillAmount = LoadingManager.Instance.Progress;
            LoadingManager.OnLoadingProgressChanged += OnLoadingProgressChanged;
        }

        private void OnLoadingProgressChanged(float progress)
        {
            progressBar.fillAmount = progress;
        }

        private void OnDisable()
        {
            LoadingManager.OnLoadingProgressChanged -= OnLoadingProgressChanged;
        }
    }
}
