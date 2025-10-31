# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Weather

> Quick overview: Drop‑in HDRP weather prefab with preconfigured Volumes for Cloud Layer, Volumetric Clouds, and Volumetric Fog. Spawn it from the menu and tweak Volume overrides to fit your scene.

A simple environment block that ships with a ready‑made Weather prefab. It uses HDRP’s Volume system to control clouds and fog, so you can get believable sky and atmosphere quickly. Use the GameObject menu to spawn the prefab, then adjust the included Volume overrides.

![screenshot](Documentation/Screenshot.png)

## Features
- Plug‑and‑play Weather prefab
  - `Resources/UnityEssentials_Prefab_Weather.prefab`
  - Contains three child Volumes:
    - Cloud Layer Volume (Cloud Layer)
    - Volumetric Clouds Volume (Volumetric Clouds)
    - Volumetric Fog Volume (Volumetric Fog)
- One‑click spawner
  - Menu: GameObject → Essentials → Weather
  - Creates the prefab in the scene and wires the Weather component to the child Volumes
- HDRP Volume friendly
  - Uses standard HDRP overrides; works with your existing Global/Local Volume setup
  - Tweak directly via the Volume components (density, altitude, lighting, wind, fog distance)

## Requirements
- Unity 6000.0+ (HDRP)
- HDRP Volumetric features enabled in your HDRP Asset
  - Cloud Layer, Volumetric Clouds, Volumetric Fog
- Volume system active in your scene (Global or Local Volumes)

## Usage

Add weather to your scene in seconds:

- From the menu: GameObject → Essentials → Weather
  - The tool spawns `UnityEssentials_Prefab_Weather.prefab` and assigns its child Volumes to the Weather component
- Or drag the prefab directly from `Assets/Repositories/Unity.Environment.Weather/Resources/`

Then tune the look:
- Select the child Volume objects (Cloud Layer / Volumetric Clouds / Volumetric Fog)
- Adjust override parameters (coverage, shape, lighting, wind, fog density/range) to match your scene
- Keep Global/Local Volume priorities in mind if you have other volumes in the scene

## Customization
- Make copies of the prefab for different weather sets (clear, cloudy, stormy) and swap or blend via your own logic
- Pair with your time-of-day system to animate cloud coverage and fog for day/night transitions
- Use reflection probes and sky/lighting settings in your Volume to enhance the result

## Notes and Limitations
- Pipeline: Built-in/URP are not supported; this prefab targets HDRP’s Volume workflow
- Underlying Weather component is expected by the spawner; it simply stores references to the child Volumes
- This module focuses on scene setup; dynamic weather transitions/FX are up to your game logic

## Files in This Package
- `Resources/UnityEssentials_Prefab_Weather.prefab` – Weather prefab with child Volumes
- `Editor/WeatherPrefabSpawner.cs` – GameObject menu spawner that instantiates and wires the prefab
- `Resources/Textures/` – Supporting textures (if any) used by the prefab/materials
- `package.json` – Package manifest metadata

## Tags
unity, hdrp, weather, clouds, volumetric, fog, cloud-layer, volumetric-clouds, volume, environment, prefab
