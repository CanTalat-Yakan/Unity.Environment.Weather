#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

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

                weather.CloudLayerVolume = prefab.transform.Find("Cloud Layer Volume").GetComponent<Volume>();
                if (weather.CloudLayerVolume.profile.TryGet<CloudLayer>(out var cloudLayerOverride))
                    weather.CloudLayerOverride = cloudLayerOverride; 

                weather.VolumetricCloudsVolume = prefab.transform.Find("Volumetric Clouds Volume").GetComponent<Volume>();
                if (weather.VolumetricCloudsVolume.profile.TryGet<VolumetricClouds>(out var volumetricCloudsOverride))
                    weather.VolumetricCloudsOverride = volumetricCloudsOverride;

                weather.VolumetricFogVolume = prefab.transform.Find("Volumetric Fog Volume").GetComponent<Volume>();
                if (weather.VolumetricFogVolume.profile.TryGet<Fog>(out var volumetricFogOverride))
                    weather.VolumetricFogOverride = volumetricFogOverride;
            }

            GameObjectUtility.SetParentAndAlign(prefab, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(prefab, "Create Weather");
            Selection.activeObject = prefab;
        }
    }
}
#endif