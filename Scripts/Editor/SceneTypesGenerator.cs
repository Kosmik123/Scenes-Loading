using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Bipolar.SceneManagement.Editor
{
    [InitializeOnLoad]
    public class SceneTypesGenerator
    {
        private const string sceneIndexFileDirectory = "Assets";
        private const string sceneIndexesClassName = "SceneIndex";

        private static readonly Regex regex = new Regex("[^a-zA-Z0-9_]");
        private static string FilePath => $"{sceneIndexFileDirectory}/{sceneIndexesClassName}.cs";

        private static int currentSceneBuildIndex;

        public class ClassTree
        {
            public string ClassName { get; private set; }
            public Dictionary<string, ClassTree> SubClasses { get; private set; } = new Dictionary<string, ClassTree>();
            public HashSet<(string, int)> Scenes { get; private set; } = new HashSet<(string, int)>();

            public ClassTree(string className)
            {
                ClassName = className;
            }

            public string GetContent(int indent = 0)
            {
                var stringBuilder = new StringBuilder();
                AppendLine(stringBuilder, $"public static class {ClassName}", indent);
                AppendLine(stringBuilder, "{", indent);
                foreach (var subClass in SubClasses)
                    stringBuilder.Append(subClass.Value.GetContent(indent + 1));

                if (Scenes.Count > 0 && SubClasses.Count > 0)
                    stringBuilder.AppendLine();

                foreach (var scene in Scenes)
                {
                    string sceneName = regex.Replace(scene.Item1, "_");
                    AppendLine(stringBuilder, $"\tpublic const int {sceneName} = {scene.Item2};", indent);
                }

                AppendLine(stringBuilder, "}", indent);
                if (indent == 1)
                    stringBuilder.AppendLine();
                return stringBuilder.ToString();

                static void AppendLine(StringBuilder builder, string line, int indent)
                {
                    for (int i = 0; i < indent; i++)
                        builder.Append('\t');
                    builder.AppendLine(line);
                }
            }
        }

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
            currentSceneBuildIndex = 0;
            var rootClassTree = new ClassTree(sceneIndexesClassName);
            var scenesArray = EditorBuildSettings.scenes;
            foreach (var scene in scenesArray)
            {
                if (scene.enabled)
                {
                    AddSceneConstantToClasses(rootClassTree, scene);
                }
            }

            return rootClassTree.GetContent();
        }

        private static void AddSceneConstantToClasses(ClassTree classTree, EditorBuildSettingsScene scene)
        {
            string scenePath = scene.path.Replace("Assets/", string.Empty);
            string[] pathDirectories = scenePath.Split('/');
            
            for (int i = 0; i < pathDirectories.Length; i++)
            {
                string directoryName = pathDirectories[i];
                if (i == pathDirectories.Length - 1)
                {
                    string sceneName = pathDirectories[i];
                    sceneName = sceneName.Substring(0, sceneName.Length - 6);
                    classTree.Scenes.Add((sceneName, currentSceneBuildIndex++));
                }
                else
                {
                    classTree = GetOrCreateClassTree(classTree.SubClasses, directoryName);
                }
            }
        }

        private static ClassTree GetOrCreateClassTree(Dictionary<string, ClassTree> classTrees, string name)
        {
            name = regex.Replace(name, "_");
            if (classTrees.TryGetValue(name, out ClassTree classTree))
                return classTree;

            var newClassTree = new ClassTree(name);
            classTrees.Add(name, newClassTree);
            return newClassTree;
        }
    }
}
