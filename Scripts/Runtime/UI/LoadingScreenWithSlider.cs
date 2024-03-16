using UnityEngine;
using UnityEngine.UI;

namespace Bipolar.SceneManagement.UI
{
    public class LoadingScreenWithSlider : LoadingScreen
    {
        [SerializeField]
        private Slider progressBar;

        protected override void OnLoadingProgressChanged(float progress)
        {
            base.OnLoadingProgressChanged(progress);
            progressBar.maxValue = 1;
            progressBar.minValue = 0;
            progressBar.value = progress;
        }
    }
}
