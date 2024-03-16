using UnityEngine;

namespace Bipolar.SceneManagement
{
    public class LoadingScreen : MonoBehaviour
    {
        private float progress;
        public float Progress
        {
            get => progress;
            set
            {
                progress = Mathf.Clamp01(value);
                OnSetProgress(progress);
            }
        }

        protected virtual void OnSetProgress(float progress) { }


        private void Awake()
        {
            
        }

        private void OnDestroy()
        {
            
        }
    }
}
