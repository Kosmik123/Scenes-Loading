using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bipolar.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(Scene))]
    public class ScenePropertyDrawer : PropertyDrawer
    {
        private const string SceneHandlePropertyName = "m_Handle";
        private static readonly System.Type SceneType = typeof(Scene);
        private static readonly MethodInfo GetNameMethodInfo = SceneType.GetMethod("GetNameInternal", BindingFlags.NonPublic | BindingFlags.Static);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneHandleProperty = property.FindPropertyRelative(SceneHandlePropertyName);
            int sceneHandle = sceneHandleProperty.intValue;
            var sceneName = GetNameMethodInfo.Invoke(default, new object[] { sceneHandle }) as string;
            EditorGUI.LabelField(position, sceneName);    
        }
    }
}
