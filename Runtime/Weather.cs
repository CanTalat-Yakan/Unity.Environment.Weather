using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEssentials
{
    [Serializable]
    public class CloudCoverType
    {
        [Tooltip("No clouds in the sky.")]
        [Range(0, 1)] public float Clear;
        [Tooltip("Few, scattered clouds.")]
        [Range(0, 1)] public float Sparse;
        [Tooltip("Many clouds, but not fully covered.")]
        [Range(0, 1)] public float Cloudy;
        [Tooltip("Sky is completely covered with clouds.")]
        [Range(0, 1)] public float Overcast;
    }

    [Serializable]
    public class PrecipitationType
    {
        [Tooltip("Light rain with small droplets.")]
        [Range(0, 1)] public float Drizzle;
        [Tooltip("Short, heavy bursts of rain.")]
        [Range(0, 1)] public float Shower;
        [Tooltip("A mix of rain and snow.")]
        [Range(0, 1)] public float Sleet;
        [Tooltip("Snowfall.")]
        [Range(0, 1)] public float Snowy;
        [Tooltip("Severe snowstorm with strong winds.")]
        [Range(0, 1)] public float Blizzard;
    }

    [Serializable]
    public class SevereWeatherType
    {
        [Tooltip("General stormy conditions, may include wind and rain.")]
        [Range(0, 1)] public float Stormy;
        [Tooltip("Storm with thunder and lightning.")]
        [Range(0, 1)] public float Thunderstorm;
        [Tooltip("Violently rotating column of air in contact with the ground.")]
        [Range(0, 1)] public float Tornado;
        [Tooltip("Large, powerful storm system with strong winds and rain.")]
        [Range(0, 1)] public float Hurricane;
        [Tooltip("Strong wind carrying sand and dust.")]
        [Range(0, 1)] public float Sandstorm;
    }

    [Serializable]
    public class AtmosphericEffectType
    {
        [Tooltip("Thick cloud near the ground reducing visibility.")]
        [Range(0, 1)] public float Foggy;
        [Tooltip("Thin, light fog.")]
        [Range(0, 1)] public float Mist;
        [Tooltip("Reduced visibility due to fine particles in the air.")]
        [Range(0, 1)] public float Hazy;
        [Tooltip("Airborne dust reducing visibility.")]
        [Range(0, 1)] public float Dusty;
        [Tooltip("Strong winds without precipitation.")]
        [Range(0, 1)] public float Windy;
    }

    [ExecuteAlways]
    public class Weather : MonoBehaviour
    {
        [HideInInspector] public Volume CloudLayerVolume;
        [HideInInspector] public CloudLayer CloudsLayerOverride;
        [HideInInspector] public Volume VolumetricCloudsVolume;
        [HideInInspector] public VolumetricClouds VolumetricCloudsOverride;
        [HideInInspector] public Volume VolumetricFogVolume;
        [HideInInspector] public Fog VolumetricFogOverride;

        public CloudCoverType CloudCover;
        public PrecipitationType Precipitation;
        public SevereWeatherType SevereWeather;
        public AtmosphericEffectType AtmosphericEffects;

        public float CloudLayerOpacity {get; private set; }
        public float VolumetricCloudsOpacity {get; private set; }
        public float FogDensity {get; private set; }
        public float CloudCoverage {get; private set; }
        public float CloudDensity { get; private set; }

        public float CameraHeight { get; private set; }

        public void Update()
        {
            UpdateWeather();
        }

        private void UpdateVolumeWeights()
        {
            CameraHeight = GetCurrentRenderingCameraHeight();

            float cloudLayerThreshold = 10_000;
            float volumetricCloudsThreshold = 50_000;

            float normCloudLayer = Mathf.Clamp01(CameraHeight / cloudLayerThreshold);
            float normVolumetric = Mathf.Clamp01(CameraHeight / volumetricCloudsThreshold);

            CloudLayerOpacity = 1 - normCloudLayer;
            VolumetricCloudsOpacity = 1 - normVolumetric;
        }

        public void UpdateWeather()
        {
            // Calculate blended weather parameters
            FogDensity = CalculateFogDensity();
            CloudCoverage = CalculateCloudCoverage();
            CloudDensity = CalculateCloudDensity();

            UpdateVolumeWeights();

            SetFogParameters();
            SetCloudParameters();
        }

        private float CalculateFogDensity()
        {
            // Base fog density from atmospheric effects
            float density = AtmosphericEffects.Foggy * 0.8f
                          + AtmosphericEffects.Mist * 0.3f
                          + AtmosphericEffects.Hazy * 0.2f;

            // Precipitation contributions to fog
            density += Precipitation.Drizzle * 0.4f
                     + Precipitation.Shower * 0.2f
                     + Precipitation.Blizzard * 0.6f;

            // Severe weather contributions
            density += SevereWeather.Sandstorm * 0.7f
                     + SevereWeather.Stormy * 0.3f;

            return Mathf.Clamp01(density);
        }

        private float CalculateCloudCoverage()
        {
            // Primary coverage from cloud types
            float coverage = CloudCover.Clear * 0.0f
                          + CloudCover.Sparse * 0.3f
                          + CloudCover.Cloudy * 0.7f
                          + CloudCover.Overcast * 1.0f;

            // Weather impacts on coverage
            coverage += SevereWeather.Stormy * 0.4f
                      + SevereWeather.Thunderstorm * 0.6f
                      + SevereWeather.Hurricane * 0.8f;

            // Precipitation impacts
            coverage += Precipitation.Blizzard * 0.5f
                      + Precipitation.Snowy * 0.3f;

            return Mathf.Clamp01(coverage);
        }

        private float CalculateCloudDensity()
        {
            // Base density from cloud types
            float density = CloudCover.Clear * 0.1f
                          + CloudCover.Sparse * 0.3f
                          + CloudCover.Cloudy * 0.6f
                          + CloudCover.Overcast * 0.9f;

            // Weather intensity impacts
            density += SevereWeather.Thunderstorm * 0.4f
                     + SevereWeather.Hurricane * 0.5f
                     + SevereWeather.Tornado * 0.3f;

            // Precipitation density contributions
            density += Precipitation.Shower * 0.3f
                     + Precipitation.Blizzard * 0.4f;

            return Mathf.Clamp01(density);
        }

        private void SetFogParameters()
        {
            // Convert density to mean free path (inverse relationship)
            float meanFreePath = Mathf.Lerp(50f, 5f, FogDensity);

            VolumetricFogOverride.enabled.Override(FogDensity > 0.01f);
            VolumetricFogOverride.meanFreePath.Override(meanFreePath);

            // Adjust fog color based on conditions
            Color fogColor = Color.Lerp(
                new Color(0.9f, 0.9f, 0.92f),
                new Color(0.7f, 0.65f, 0.6f),
                AtmosphericEffects.Dusty + SevereWeather.Sandstorm);
            VolumetricFogOverride.albedo.Override(fogColor);
        }

        private void SetCloudParameters()
        {
            // Volumetric clouds
            //VolumetricCloudsOverride.cloudCoverage.Override(CloudCoverage);
            //VolumetricCloudsOverride.densityMultiplier.Override(
            //    Mathf.Lerp(0.2f, 1.5f, CloudDensity));

            // Cloud layers
            //CloudsLayerOverride.densityMultiplier.Override(CloudDensity * 0.8f);
            //CloudsLayerOverride.opacity.Override(CloudCoverage * 0.9f);

            VolumetricCloudsOverride.densityMultiplier.Override(VolumetricCloudsOpacity);
            CloudsLayerOverride.opacity.Override(CloudLayerOpacity);

            // Darken clouds during severe weather
            float darkenAmount = Mathf.Clamp01(
                SevereWeather.Thunderstorm + SevereWeather.Hurricane);

            Color cloudColor = Color.Lerp(
                Color.white, 
                new Color(0.4f, 0.4f, 0.45f), 
                darkenAmount);
            VolumetricCloudsOverride.scatteringTint.Override(cloudColor);
        }

        private float GetCurrentRenderingCameraHeight()
        {
#if UNITY_EDITOR
            // Prefer SceneView camera if available and focused
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera != null && sceneView.hasFocus)
                return Mathf.Max(100, sceneView.camera.transform.position.magnitude);
#endif
            // Fallback to main camera
            if (Camera.main != null)
                return Mathf.Max(100, Camera.main.transform.position.magnitude);

            return 0f;
        }
    }
}
