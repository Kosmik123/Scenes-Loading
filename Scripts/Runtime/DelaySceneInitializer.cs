using UnityEngine;

namespace Bipolar.SceneManagement
{
    public class DelaySceneInitializer : SceneInitializer
    {
        [SerializeField]
        private float loadingDuration;
        private float timer;

        public override float InitializationProgress => Mathf.Clamp01(timer / loadingDuration);

        private void Update()
        {
            timer += Time.deltaTime;
        }
    }

    public abstract class LoadingStrategy : ScriptableObject
    {
    }
}
