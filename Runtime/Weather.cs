using System;
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


    }
}
