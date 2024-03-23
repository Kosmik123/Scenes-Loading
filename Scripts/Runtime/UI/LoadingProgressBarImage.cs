using UnityEngine;
using UnityEngine.UI;

namespace Bipolar.SceneManagement.UI
{
    public class LoadingProgressBarImage : MonoBehaviour
    {
        [SerializeField]
        private Image progressBar;

        private float targetProgress;

        private void OnEnable()
        {
            progressBar.fillAmount = targetProgress = LoadingManager.Progress;
            LoadingManager.OnLoadingProgressChanged += OnLoadingProgressChanged;
        }

        private void Update()
        {
            progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, targetProgress, Time.deltaTime);
        }

        private void OnLoadingProgressChanged(float progress)
        {
            targetProgress = progress;
        }

        private void OnDisable()
        {
            LoadingManager.OnLoadingProgressChanged -= OnLoadingProgressChanged;
        }
    }
}
