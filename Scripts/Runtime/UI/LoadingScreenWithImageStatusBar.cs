using UnityEngine;
using UnityEngine.UI;

namespace Bipolar.SceneManagement.UI
{
    public class LoadingScreenWithImageStatusBar : LoadingScreen
    {
        [SerializeField]
        private Image progressBar;

        protected override void OnLoadingProgressChanged(float progress)
        {
            base.OnLoadingProgressChanged(progress);
            progressBar.fillAmount = progress;
        }
    }
}
