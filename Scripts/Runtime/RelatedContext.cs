using UnityEngine;

namespace Bipolar.SceneManagement
{
    [DisallowMultipleComponent]
    public class RelatedContext : MonoBehaviour
    {
        [SerializeField]
        private ScenesContext context;

        private void Awake()
        {
            if (LoadingManager.Instance == null)
            {
                LoadingManager.OnInstanceCreated += LoadContext;
            }
            else if (LoadingManager.Instance.IsLoading)
            {
                LoadingManager.OnLoadingEnded += LoadContext;
            }
            else
            {
                LoadContext();
            }
        }


        private void LoadContext()
        {
            LoadingManager.OnLoadingEnded -= LoadContext;
            LoadingManager.OnInstanceCreated -= LoadContext;
            if (LoadingManager.Instance.CurrentContext != context)
            {
                LoadingManager.Instance.LoadContext(context);
            }
        }
    } 
}
