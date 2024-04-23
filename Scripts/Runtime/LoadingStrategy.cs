using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.SceneManagement
{
    public abstract class LoadingStrategy : ScriptableObject
    {
        public abstract IEnumerable<float> LoadContext(ScenesContext context);
    }

    public class DefaultLoadingStrategy : LoadingStrategy
    {
        public static DefaultLoadingStrategy Instance { get; } = CreateInstance<DefaultLoadingStrategy>();

        public override IEnumerable<float> LoadContext(ScenesContext context)
        {
            foreach (var progress in LoadScenesFromDisk(context))
                yield return progress;
            
            yield return 0.5f;

            foreach (var progress in InitializeScenes())
                yield return progress;
        }

        public static IEnumerable<float> LoadScenesFromDisk(ScenesContext context)
        {
            yield return default;

        }

        public static IEnumerable<float> InitializeScenes()
        {
            float progress = 0.5f;
            while (progress < 1)
            {
                progress += 0.01f;
                yield return progress;
            }
        }

    }
}