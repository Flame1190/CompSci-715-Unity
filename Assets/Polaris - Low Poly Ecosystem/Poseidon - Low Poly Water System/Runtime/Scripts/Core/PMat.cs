using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

namespace Pinwheel.Poseidon
{
    public class PMat
    {
        public static readonly string KW_LIGHTING_BLINN_PHONG = "LIGHTING_BLINN_PHONG";
        public static readonly string KW_LIGHTING_LAMBERT = "LIGHTING_LAMBERT";

        public static readonly string KW_FLAT_LIGHTING = "FLAT_LIGHTING";

        public static readonly int QUEUE_TRANSPARENT = 3000;

        public static readonly int COLOR = Shader.PropertyToID("_Color");
        public static readonly int SPEC_COLOR = Shader.PropertyToID("_Specular");
        public static readonly int SPEC_COLOR_BLINN_PHONG = Shader.PropertyToID("_SpecColor");
        public static readonly int SMOOTHNESS = Shader.PropertyToID("_Smoothness");

        public static readonly string KW_MESH_NOISE = "MESH_NOISE";
        public static readonly int MESH_NOISE = Shader.PropertyToID("_MeshNoise");

        public static readonly string KW_LIGHT_ABSORPTION = "LIGHT_ABSORPTION";
        public static readonly int DEPTH_COLOR = Shader.PropertyToID("_DepthColor");
        public static readonly int MAX_DEPTH = Shader.PropertyToID("_MaxDepth");
        
        public static readonly string KW_FOAM = "FOAM";
        public static readonly string KW_FOAM_HQ = "FOAM_HQ";
        public static readonly string KW_FOAM_CREST = "FOAM_CREST";
        public static readonly string KW_FOAM_SLOPE = "FOAM_SLOPE";
        public static readonly int FOAM_COLOR = Shader.PropertyToID("_FoamColor");
        public static readonly int FOAM_DISTANCE = Shader.PropertyToID("_FoamDistance");
        public static readonly int FOAM_NOISE_SCALE_HQ = Shader.PropertyToID("_FoamNoiseScaleHQ");
        public static readonly int FOAM_NOISE_SPEED_HQ = Shader.PropertyToID("_FoamNoiseSpeedHQ");
        public static readonly int SHORELINE_FOAM_STRENGTH = Shader.PropertyToID("_ShorelineFoamStrength");
        public static readonly int CREST_FOAM_STRENGTH = Shader.PropertyToID("_CrestFoamStrength");
        public static readonly int CREST_MAX_DEPTH = Shader.PropertyToID("_CrestMaxDepth");
        public static readonly int SLOPE_FOAM_STRENGTH = Shader.PropertyToID("_SlopeFoamStrength");
        public static readonly int SLOPE_FOAM_FLOW_SPEED = Shader.PropertyToID("_SlopeFoamFlowSpeed");
        public static readonly int SLOPE_FOAM_DISTANCE = Shader.PropertyToID("_SlopeFoamDistance");

        public static readonly int RIPPLE_HEIGHT = Shader.PropertyToID("_RippleHeight");
        public static readonly int RIPPLE_SPEED = Shader.PropertyToID("_RippleSpeed");
        public static readonly int RIPPLE_NOISE_SCALE = Shader.PropertyToID("_RippleNoiseScale");

        public static readonly string KW_WAVE = "WAVE";
        public static readonly int WAVE_DIRECTION = Shader.PropertyToID("_WaveDirection");
        public static readonly int WAVE_SPEED = Shader.PropertyToID("_WaveSpeed");
        public static readonly int WAVE_HEIGHT = Shader.PropertyToID("_WaveHeight");
        public static readonly int WAVE_LENGTH = Shader.PropertyToID("_WaveLength");
        public static readonly int WAVE_STEEPNESS = Shader.PropertyToID("_WaveSteepness");
        public static readonly int WAVE_DEFORM = Shader.PropertyToID("_WaveDeform");
        public static readonly string KW_WAVE_MASK = "WAVE_MASK";
        public static readonly int WAVE_MASK = Shader.PropertyToID("_WaveMask");
        public static readonly int WAVE_MASK_BOUNDS = Shader.PropertyToID("_WaveMaskBounds");

        public static readonly int FRESNEL_STRENGTH = Shader.PropertyToID("_FresnelStrength");
        public static readonly int FRESNEL_BIAS = Shader.PropertyToID("_FresnelBias");

        public static readonly string KW_REFLECTION = "REFLECTION";
        public static readonly string KW_REFLECTION_BLUR = "REFLECTION_BLUR";
        public static readonly int REFLECTION_TEX = Shader.PropertyToID("_ReflectionTex");
        public static readonly int REFLECTION_DISTORTION_STRENGTH = Shader.PropertyToID("_ReflectionDistortionStrength");

        public static readonly string KW_REFRACTION = "REFRACTION";
        public static readonly int REFRACTION_TEX = Shader.PropertyToID("_RefractionTex");
        public static readonly int REFRACTION_DISTORTION_STRENGTH = Shader.PropertyToID("_RefractionDistortionStrength");

        public static readonly string KW_CAUSTIC = "CAUSTIC";
        public static readonly int CAUSTIC_TEX = Shader.PropertyToID("_CausticTex");
        public static readonly int CAUSTIC_SIZE = Shader.PropertyToID("_CausticSize");
        public static readonly int CAUSTIC_STRENGTH = Shader.PropertyToID("_CausticStrength");
        public static readonly int CAUSTIC_DISTORTION_STRENGTH = Shader.PropertyToID("_CausticDistortionStrength");

        public static readonly string KW_BACK_FACE = "BACK_FACE";

        public static readonly int NOISE_TEX = Shader.PropertyToID("_NoiseTex");
        public static readonly int TIME = Shader.PropertyToID("_PoseidonTime");
        public static readonly int SINE_TIME = Shader.PropertyToID("_PoseidonSineTime");

        public static readonly int PP_NOISE_TEX = Shader.PropertyToID("_NoiseTex");
        public static readonly int PP_INTENSITY = Shader.PropertyToID("_Intensity");

        public static readonly int PP_WATER_LEVEL = Shader.PropertyToID("_WaterLevel");
        public static readonly int PP_MAX_DEPTH = Shader.PropertyToID("_MaxDepth");
        public static readonly int PP_SURFACE_COLOR_BOOST = Shader.PropertyToID("_SurfaceColorBoost");

        public static readonly int PP_SHALLOW_FOG_COLOR = Shader.PropertyToID("_ShallowFogColor");
        public static readonly int PP_DEEP_FOG_COLOR = Shader.PropertyToID("_DeepFogColor");
        public static readonly int PP_VIEW_DISTANCE = Shader.PropertyToID("_ViewDistance");

        public static readonly string KW_PP_CAUSTIC = "CAUSTIC";
        public static readonly int PP_CAUSTIC_TEX = Shader.PropertyToID("_CausticTex");
        public static readonly int PP_CAUSTIC_SIZE = Shader.PropertyToID("_CausticSize");
        public static readonly int PP_CAUSTIC_STRENGTH = Shader.PropertyToID("_CausticStrength");

        public static readonly string KW_PP_DISTORTION = "DISTORTION";
        public static readonly int PP_DISTORTION_TEX = Shader.PropertyToID("_DistortionTex");
        public static readonly int PP_DISTORTION_STRENGTH = Shader.PropertyToID("_DistortionStrength");
        public static readonly int PP_WATER_FLOW_SPEED = Shader.PropertyToID("_WaterFlowSpeed");

        public static readonly int PP_CAMERA_VIEW_DIR = Shader.PropertyToID("_CameraViewDir");
        public static readonly int PP_CAMERA_FOV = Shader.PropertyToID("_CameraFov");
        public static readonly int PP_CAMERA_TO_WORLD_MATRIX = Shader.PropertyToID("_CameraToWorldMatrix");

        public static readonly int PP_WET_LENS_TEX = Shader.PropertyToID("_WetLensTex");
        public static readonly int PP_WET_LENS_STRENGTH = Shader.PropertyToID("_Strength");

        public static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static Material activeMaterial;

        public static void SetActiveMaterial(Material mat)
        {
            activeMaterial = mat;
        }

        public static void GetColor(int prop, ref Color value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    value = activeMaterial.GetColor(prop);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void GetFloat(int prop, ref float value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    value = activeMaterial.GetFloat(prop);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void GetVector(int prop, ref Vector4 value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    value = activeMaterial.GetVector(prop);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void GetTexture(int prop, ref Texture value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    value = activeMaterial.GetTexture(prop);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void GetKeywordEnabled(string kw, ref bool value)
        {
            try
            {
                value = activeMaterial.IsKeywordEnabled(kw);
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetColor(int prop, Color value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    activeMaterial.SetColor(prop, value);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetFloat(int prop, float value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    activeMaterial.SetFloat(prop, value);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetVector(int prop, Vector4 value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    activeMaterial.SetVector(prop, value);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetTexture(int prop, Texture value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    activeMaterial.SetTexture(prop, value);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }
        
        public static void SetKeywordEnable(string kw, bool enable)
        {
            try
            {
                if (enable)
                {
                    activeMaterial.EnableKeyword(kw);
                }
                else
                {
                    activeMaterial.DisableKeyword(kw);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetOverrideTag(string tag, string value)
        {
            activeMaterial.SetOverrideTag(tag, value);
        }

        public static void SetRenderQueue(int queue)
        {
            activeMaterial.renderQueue = queue;
        }

        public static void SetRenderQueue(RenderQueue queue)
        {
            activeMaterial.renderQueue = (int)queue;
        }

        public static void SetSourceBlend(BlendMode mode)
        {
            activeMaterial.SetInt("_SrcBlend", (int)mode);
        }

        public static void SetDestBlend(BlendMode mode)
        {
            activeMaterial.SetInt("_DstBlend", (int)mode);
        }

        public static void SetZWrite(bool value)
        {
            activeMaterial.SetInt("ZWrite", value ? 1 : 0);
        }

        public static void SetBlend(bool value)
        {
            activeMaterial.SetInt("_Blend", value ? 1 : 0);
        }

        public static void SetShader(Shader shader)
        {
            int queue = activeMaterial.renderQueue;
            activeMaterial.shader = shader;
            activeMaterial.renderQueue = queue;
        }

        public static void SetFeature(ref bool feature, bool value)
        {
            if (feature != value)
            {
#if UNITY_EDITOR
                feature = value;
#else
                Debug.LogError($"Water features cannot be changed at runtime. Try creating some water prefabs with different feature set, put them all in a dummy scene, include the scene in the build, then switch between prefabs using script.");
#endif
            }
        }
    }
}