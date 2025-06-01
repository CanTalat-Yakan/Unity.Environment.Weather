#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    public class WeatherEditor
    {
        [MenuItem("GameObject/Essentials/Weather", false, priority = 101)]
        private static void InstantiateTimeOfDay(MenuCommand menuCommand)
        {
            var prefab = ResourceLoaderEditor.InstantiatePrefab("UnityEssentials_Prefab_Weather", "Weather");
            if (prefab != null)
            {
                var weather = prefab.GetComponent<Weather>();
            }

            GameObjectUtility.SetParentAndAlign(prefab, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(prefab, "Create Weather");
            Selection.activeObject = prefab;
        }
    }
}
#endif