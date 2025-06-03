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

        public void Update() =>
            UpdateWeather();

        public void UpdateWeather()
        {
            // Calculate blended weather parameters
            VolumetricCloudsCoverage = CalculateCloudCoverage();
            VolumetricCloudsDensity = CalculateCloudDensity();
            CloudLayerDensity = 1;
            CloudLayerCoverage = 1;

            TimeOfDay.CloudCoverage = VolumetricCloudsCoverage;

            UpdateCloudOpacities();

            SetFogParameters();
            SetCloudParameters();
        }

        private void UpdateCloudOpacities()
        {
            const float cloudLayerThreshold = 10_000;
            const float volumetricCloudsThreshold = 50_000;

            var cloudLayerOpacity = 1 - Mathf.Clamp01(TimeOfDay.CameraDistance / cloudLayerThreshold);
            var volumetricCloudsOpacity = 1 - Mathf.Clamp01(TimeOfDay.CameraDistance / volumetricCloudsThreshold);

            // Fades out clouds based on camera height when entering outer space
            VolumetricCloudsDensity *= volumetricCloudsOpacity;
            CloudLayerDensity *= cloudLayerOpacity;
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

        private Color _fogBrightColor = Color.white;
        private Color _fogDustyColor = new Color(0.82f, 0.6f, 0.4f);
        private void SetFogParameters()
        {
            // Base (clear) values
            const float baseDistance = 2000f;
            const float baseBaseHeight = -50f;
            const float baseFogHeight = 50f;

            // Target values for each effect at full strength
            const float foggyDistance = 50f, foggyBaseHeight = 100f, foggyFogHeight = 250f;
            const float mistDistance = 200f, mistBaseHeight = 5f, mistFogHeight = 80f;
            const float hazyDistance = 200f, hazyBaseHeight = 40f, hazyFogHeight = 120f;

            // Effect weights
            float foggy = AtmosphericEffects.Foggy;
            float mist = AtmosphericEffects.Mist;
            float hazy = AtmosphericEffects.Hazy;

            float total = foggy + mist + hazy;
            float clearWeight = 1f - Mathf.Clamp01(total);

            // Weighted blend
            float meanFreePath =
                baseDistance * clearWeight +
                foggyDistance * foggy +
                mistDistance * mist +
                hazyDistance * hazy;

            float baseHeight =
                baseBaseHeight * clearWeight +
                foggyBaseHeight * foggy +
                mistBaseHeight * mist +
                hazyBaseHeight * hazy;

            float fogHeight =
                baseFogHeight * clearWeight +
                foggyFogHeight * foggy +
                mistFogHeight * mist +
                hazyFogHeight * hazy;

            // Optionally, add Dusty/Sandstorm effects to color
            var dustyContribution = AtmosphericEffects.Dusty + SevereWeather.Sandstorm;
            var fogColor = Color.Lerp(_fogBrightColor, _fogDustyColor, dustyContribution);

            // Apply to volume overrides
            VolumetricFogOverride.meanFreePath.Override(meanFreePath);
            VolumetricFogOverride.baseHeight.Override(baseHeight);
            VolumetricFogOverride.maximumHeight.Override(fogHeight);
            VolumetricFogOverride.albedo.Override(fogColor);
            // Reduces blur effect caused by wet fog
            VolumetricFogOverride.multipleScatteringIntensity.Override(1 - AtmosphericEffects.Dusty); 

            // Workaround to prevent fog on the horizon from appearing in front of buildings
            VolumetricFogOverride.mipFogMaxMip.Override(Mathf.Clamp01(TimeOfDay.CameraHeight / 100) / 2);
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
            VolumetricCloudsOverride.enable.Override(Settings.EnableVolumetricClouds);

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
