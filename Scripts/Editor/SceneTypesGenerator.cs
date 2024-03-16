using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Bipolar.SceneManagement.Editor
{
    //[InitializeOnLoad]
    public class SceneTypesGenerator
    {
        private const string sceneIndexFileDirectory = "Assets";
        private const string sceneIndexesClassName = "SceneIndex";
        private static readonly Regex regex = new Regex("[^a-zA-Z0-9_]");

        private static string FilePath => $"{sceneIndexFileDirectory}/{sceneIndexesClassName}.cs";

        static SceneTypesGenerator()
        {
            EditorBuildSettings.sceneListChanged -= Generate;
            EditorBuildSettings.sceneListChanged += Generate;
            if (File.Exists(FilePath) == false)
            {
                Generate();
                return;
            }
            string oldClassContent = File.ReadAllText(FilePath);
            string newClassContent = GetClassContent();
            if (newClassContent != oldClassContent)
            {
                Generate(newClassContent);
            }
        }

        private static void Generate()
        {
            Generate(GetClassContent());
        }

        private static void Generate(string content)
        {
            AssetDatabase.MakeEditable(FilePath);
            File.WriteAllText(FilePath, content);
            AssetDatabase.ImportAsset(FilePath);
            AssetDatabase.SaveAssets();
        }

        private static string GetClassContent()
        {
            var scenesArray = EditorBuildSettings.scenes;

            var builder = new StringBuilder();
            builder.AppendLine($"public static class {sceneIndexesClassName}");
            builder.AppendLine("{");

            int buildIndex = 0;
            foreach (var scene in scenesArray)
            {
                if (scene.enabled)
                {
                    var name = scene.path.Replace("Assets/", string.Empty);
                    name = name.Replace(" ", string.Empty);
                    name = regex.Replace(name, "_");
                    name = name.Substring(0, name.Length - 6);
                    builder.AppendLine($"\tpublic const int {name} = {buildIndex};");
                    buildIndex++;
                }
            }
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
