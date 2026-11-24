using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bipolar.SceneManagement.Editor
{
    /// <summary>
    /// Personal (not shared through version control) choice of the context loaded
    /// when Play mode is started from a scene that belongs to more than one context.
    /// Stored in UserSettings, keyed by scene and context asset GUIDs.
    /// </summary>
    [FilePath("UserSettings/Bipolar.SceneManagement.PlayModeContexts.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class PlayModeContextChoices : ScriptableSingleton<PlayModeContextChoices>
    {
        [System.Serializable]
        private class Choice
        {
            [SerializeField]
            private string sceneGuid;
            public string SceneGuid => sceneGuid;

            [SerializeField]
            private string contextGuid;
            public string ContextGuid
            {
                get => contextGuid;
                set => contextGuid = value;
            }

            public Choice(string sceneGuid, string contextGuid)
            {
                this.sceneGuid = sceneGuid;
                this.contextGuid = contextGuid;
            }
        }

        [SerializeField]
        private List<Choice> choices = new List<Choice>();

        public bool TryGetContextGuid(string sceneGuid, out string contextGuid)
        {
            foreach (var choice in choices)
            {
                if (choice.SceneGuid != sceneGuid)
                    continue;

                contextGuid = choice.ContextGuid;
                return true;
            }

            contextGuid = null;
            return false;
        }

        public void SetChoice(string sceneGuid, string contextGuid)
        {
            var existingChoice = choices.Find(choice => choice.SceneGuid == sceneGuid);
            if (existingChoice == null)
                choices.Add(new Choice(sceneGuid, contextGuid));
            else
                existingChoice.ContextGuid = contextGuid;

            Save(true);
        }
    }
}
