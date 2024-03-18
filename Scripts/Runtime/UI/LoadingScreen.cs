using UnityEngine;

namespace Bipolar.SceneManagement.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        private void Awake()
        {
            var methodToCall = LoadingManager.Instance.IsLoading ? (System.Action)OnLoadingStarted : OnLoadingEnded;
            methodToCall.Invoke();

            LoadingManager.OnLoadingStarted += OnLoadingStarted;
            LoadingManager.OnLoadingEnded += OnLoadingEnded;
        }

        private void OnLoadingStarted() 
        {
            gameObject.SetActive(true);    
        }

        private void OnLoadingEnded() 
        {
            gameObject.SetActive(false);    
        }

        private void OnDestroy()
        {
            LoadingManager.OnLoadingStarted -= OnLoadingStarted;
            LoadingManager.OnLoadingEnded -= OnLoadingEnded;
        }
    }
}
