namespace Bipolar.SceneManagement.Core
{
    public interface IInitializer
    {
        float InitializationProgress { get; }
        bool IsInitialized { get; }
    }
}
