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

    [Serializable]
    public class CloudsSettings
    {
        public bool EnableVolumetricClouds = true;
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

        public CloudsSettings Settings;
        public CloudCoverType CloudCover;
        public PrecipitationType Precipitation;
        public SevereWeatherType SevereWeather;
        public AtmosphericEffectType AtmosphericEffects;

        public float FogDensity { get; private set; }
        public float VolumetricCloudsCoverage { get; private set; }
        public float VolumetricCloudsDensity { get; private set; }
        public float CloudLayerCoverage { get; private set; }
        public float CloudLayerDensity { get; private set; }

        public TimeOfDay TimeOfDay => _timeOfDay ??= TimeOfDay.Instance;
        private TimeOfDay _timeOfDay;


        public void Update()
        {
            UpdateWeather();
        }

        private void UpdateVolumeWeights()
        {
            const float cloudLayerThreshold = 10_000;
            const float volumetricCloudsThreshold = 50_000;

            var cloudLayerOpacity = 1 - Mathf.Clamp01(TimeOfDay.CameraHeight / cloudLayerThreshold);
            var volumetricCloudsOpacity = 1 - Mathf.Clamp01(TimeOfDay.CameraHeight / volumetricCloudsThreshold);

            // Fades out clouds based on camera height when entering outer space
            VolumetricCloudsDensity *= volumetricCloudsOpacity;
            CloudLayerDensity *= cloudLayerOpacity;
        }

        public void UpdateWeather()
        {
            // Calculate blended weather parameters
            FogDensity = CalculateFogDensity();
            VolumetricCloudsCoverage = CalculateCloudCoverage();
            VolumetricCloudsDensity = CalculateCloudDensity();
            CloudLayerCoverage = 1;
            CloudLayerDensity = 1;

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
            float density = (1 - CloudCover.Clear) * 0.1f
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

        private Color _fogBrightColor = new Color(0.9f, 0.9f, 0.92f);
        private Color _fogDustyColor = new Color(0.7f, 0.65f, 0.6f);
        private void SetFogParameters()
        {
            // Convert density to mean free path (inverse relationship)
            float meanFreePath = Mathf.Lerp(50f, 5f, FogDensity);

            // Fog distance attenuation: more foggy = less distance
            float fogAttenuation = Mathf.Lerp(1.0f, 0.3f, AtmosphericEffects.Foggy);
            meanFreePath *= fogAttenuation;

            // Base height and height for low fog
            // Low fog: base is near ground, height is small; more foggy = thicker fog
            float lowFog = AtmosphericEffects.Foggy + AtmosphericEffects.Mist;
            float baseHeight = Mathf.Lerp(10f, 0.5f, lowFog); // Lower base for more low fog
            float fogHeight = Mathf.Lerp(30f, 120f, lowFog);  // Thicker fog for more low fog

            // Block more visibility for higher values
            if (lowFog > 0.5f)
            {
                // For very dense fog, make it even thicker and lower
                baseHeight = Mathf.Lerp(baseHeight, 0.1f, (lowFog - 0.5f) * 2f);
                fogHeight = Mathf.Lerp(fogHeight, 200f, (lowFog - 0.5f) * 2f);
            }

            VolumetricFogOverride.meanFreePath.Override(meanFreePath);

            // Set base height and height if available
            if (VolumetricFogOverride.baseHeight != null)
                VolumetricFogOverride.baseHeight.Override(baseHeight);
            if (VolumetricFogOverride.maximumHeight != null)
                VolumetricFogOverride.maximumHeight.Override(fogHeight);

            // Adjust fog color based on conditions
            var dustyContribution = AtmosphericEffects.Dusty + SevereWeather.Sandstorm;
            Color fogColor = Color.Lerp(_fogBrightColor, _fogDustyColor, dustyContribution);
            VolumetricFogOverride.albedo.Override(fogColor);
        }

        private Color _colorBright = Color.white;
        private Color _colorDark = new Color(0.4f, 0.4f, 0.45f);
        private void SetCloudParameters()
        {
            // Update cloud coverage and density based on weather conditions
            VolumetricCloudsOverride.shapeFactor.Override(VolumetricCloudsCoverage.Remap(0, 1, 1, 0.5f));
            VolumetricCloudsOverride.densityMultiplier.Override(VolumetricCloudsDensity);
            // Eliminates blue tint from clouds at night
            VolumetricCloudsOverride.ambientLightProbeDimmer.Override(TimeOfDay.DayWeight);

            CloudsLayerOverride.opacity.Override(CloudLayerDensity);
            CloudsLayerOverride.layerA.opacityR.Override(CloudCover.Sparse * CloudLayerCoverage);
            CloudsLayerOverride.layerA.opacityG.Override(CloudCover.Cloudy * CloudLayerCoverage);
            CloudsLayerOverride.layerA.opacityB.Override(1 - CloudCover.Clear * CloudLayerCoverage);
            CloudsLayerOverride.layerA.opacityA.Override(CloudCover.Overcast * CloudLayerCoverage);
            // At least one cloud system should be casting shadows
            CloudsLayerOverride.shadowMultiplier.Override(Settings.EnableVolumetricClouds ? 0 : 1);

            // Darken clouds during severe weather
            var darkenAmount = Mathf.Clamp01(SevereWeather.Thunderstorm + SevereWeather.Hurricane);
            var cloudColor = Color.Lerp(_colorBright, _colorDark, darkenAmount);
            VolumetricCloudsOverride.scatteringTint.Override(cloudColor);
        }
    }
}
