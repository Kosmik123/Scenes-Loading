using UnityEngine;
using UnityEngine.UI;

namespace Bipolar.SceneManagement
{
    public class LoadingScreenWithImageStatusBar : LoadingScreen
    {
        [SerializeField]
        private Image progressBar;

        protected override void OnSetProgress(float progress)
        {
            base.OnSetProgress(progress);
            progressBar.fillAmount = progress;
        }
    }
}
