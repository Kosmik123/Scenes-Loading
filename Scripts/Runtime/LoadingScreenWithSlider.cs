using UnityEngine;
using UnityEngine.UI;

namespace Bipolar.SceneManagement
{
    public class LoadingScreenWithSlider: LoadingScreen
    {
        [SerializeField]
        private Slider progressBar;

        protected override void OnSetProgress(float progress)
        {
            base.OnSetProgress(progress);
            progressBar.maxValue = 1;
            progressBar.minValue = 0;
            progressBar.value = progress;
        }
    }
}
