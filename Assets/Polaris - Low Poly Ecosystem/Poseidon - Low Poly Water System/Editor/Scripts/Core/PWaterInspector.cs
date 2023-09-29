using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.Poseidon.FX;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
#if TEXTURE_GRAPH
using Pinwheel.TextureGraph;
#endif

namespace Pinwheel.Poseidon
{
    [CustomEditor(typeof(PWater))]
    public class PWaterInspector : Editor
    {
        private PWater water;
        private bool willDrawDebugGUI = false;

        private readonly int[] renderTextureSizes = new int[] { 128, 256, 512, 1024, 2048 };
        private readonly string[] renderTextureSizeLabels = new string[] { "128", "256", "512", "1024", "2048*" };

        private readonly int[] meshTypes = new int[]
        {
            (int)PWaterMeshType.TileablePlane,
            (int)PWaterMeshType.Area,
            (int)PWaterMeshType.Spline,
            (int)PWaterMeshType.CustomMesh
        };
        private readonly string[] meshTypeLabels = new string[]
        {
            "Tilealbe Plane",
            "Area (Experimental)",
            "Spline (Experimental)",
            "Custom (Experimental)"
        };

        private bool isEditingTileIndices = false;
        private bool isEditingAreaMesh = false;
        private bool isEditingSplineMesh = false;

        private PTilesEditingGUIDrawer tileEditingGUIDrawer;
        private PAreaEditingGUIDrawer areaEditingGUIDrawer;
        private PSplineEditingGUIDrawer splineEditingGUIDrawer;

        private static Mesh quadMesh;
        private static Mesh QuadMesh
        {
            get
            {
                if (quadMesh == null)
                {
                    quadMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
                }
                return quadMesh;
            }
        }

        private static Material maskVisualizerMaterial;
        private static Material MaskVisualizerMaterial
        {
            get
            {
                if (maskVisualizerMaterial == null)
                {
                    maskVisualizerMaterial = new Material(Shader.Find("Hidden/Poseidon/WaveMaskVisualizer"));
                }
                return maskVisualizerMaterial;
            }
        }

        private enum PWaveMaskVisualizationMode
        {
            None,
            //Flow, 
            Crest,
            Height
        }

        private PWaveMaskVisualizationMode waveMaskVisMode;

        private void OnEnable()
        {
            LoadPrefs();
            water = target as PWater;
            if (water.Profile != null)
            {
                water.ReCalculateBounds();
            }

            tileEditingGUIDrawer = new PTilesEditingGUIDrawer(water);
            areaEditingGUIDrawer = new PAreaEditingGUIDrawer(water);
            splineEditingGUIDrawer = new PSplineEditingGUIDrawer(water);

            SceneView.duringSceneGui += DuringSceneGUI;
            Camera.onPreCull += OnRenderCamera;
            RenderPipelineManager.beginCameraRendering += OnRenderCameraSRP;
        }

        private void OnDisable()
        {
            SavePrefs();
            isEditingTileIndices = false;
            if (isEditingAreaMesh)
            {
                isEditingAreaMesh = false;
                water.GenerateAreaMesh();
            }
            if (isEditingSplineMesh)
            {
                isEditingSplineMesh = false;
                water.GenerateSplineMesh();
            }

            SceneView.duringSceneGui -= DuringSceneGUI;
            Camera.onPreCull -= OnRenderCamera;
            RenderPipelineManager.beginCameraRendering -= OnRenderCameraSRP;
        }

        private void LoadPrefs()
        {
            waveMaskVisMode = (PWaveMaskVisualizationMode)SessionState.GetInt("poseidon-wave-mask-vis-mode", 0);
        }

        private void SavePrefs()
        {
            SessionState.SetInt("poseidon-wave-mask-vis-mode", (int)waveMaskVisMode);
        }

        public override void OnInspectorGUI()
        {
            if (water.transform.rotation != Quaternion.identity)
            {
                string warning = "The water object is designed to work without rotation. Some features may not work correctly.";
                EditorGUILayout.LabelField(warning, PEditorCommon.WarningLabel);
            }

            DrawProfileGUI();
            if (water.Profile != null)
            {
                EditorGUI.BeginChangeCheck();
                DrawMeshSettingsGUI();
                DrawRenderingSettingsGUI();
                DrawTimeSettingsGUI();
                DrawColorsSettingsGUI();
                DrawFresnelSettingsGUI();
                DrawRippleSettingsGUI();
                DrawWaveSettingsGUI();
                DrawLightAbsorbtionSettingsGUI();
                DrawFoamSettingsGUI();
                DrawReflectionSettingsGUI();
                DrawRefractionSettingsGUI();
                DrawCausticSettingsGUI();
                DrawEffectsGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(water);
                    EditorUtility.SetDirty(water.Profile);
                }
            }
        }

        private void DrawProfileGUI()
        {
            EditorGUI.BeginChangeCheck();
            PWaterProfile profile = PEditorCommon.ScriptableObjectField<PWaterProfile>("Profile", water.Profile);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(water, "Change water profile");
                water.Profile = profile;
            }
        }

        private void DrawEffectsGUI()
        {
            PWaterFX fx = water.GetComponent<PWaterFX>();
            if (fx != null)
                return;

            string label = "Effects";
            string id = "effects" + water.GetInstanceID();

            PEditorCommon.Foldout(label, false, id, () =>
            {
                GUI.enabled = true;
                if (PCommon.CurrentRenderPipeline == PRenderPipelineType.Builtin)
                {
                    bool isStackV2Installed = false;
#if UNITY_POST_PROCESSING_STACK_V2
                isStackV2Installed = true;
#endif
                    if (!isStackV2Installed)
                    {
                        EditorGUILayout.LabelField("Water effect need the Post Processing Stack V2 to work. Please install it using the Package Manager", PEditorCommon.WordWrapItalicLabel);
                    }
                    GUI.enabled = isStackV2Installed;
                }
                if (GUILayout.Button("Add Effects"))
                {
                    fx = water.gameObject.AddComponent<PWaterFX>();
                    fx.Water = water;
                }
                GUI.enabled = true;
            });
        }

        private void DrawMeshSettingsGUI()
        {
            string label = "Mesh";
            string id = "water-profile-mesh";
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Generate"),
                false,
                () => { water.GenerateMesh(); });

            PEditorCommon.Foldout(label, true, id, () =>
            {
                if (water.MeshType == PWaterMeshType.TileablePlane)
                {
                    DrawTilableMeshGUI();
                }
                else if (water.MeshType == PWaterMeshType.Area)
                {
                    DrawAreaMeshGUI();
                }
                else if (water.MeshType == PWaterMeshType.Spline)
                {
                    DrawSplineMeshGUI();
                }
                else if (water.MeshType == PWaterMeshType.CustomMesh)
                {
                    DrawCustomMeshGUI();
                }
            }, menu);
        }

        private void DrawTilableMeshGUI()
        {
            if (!isEditingTileIndices)
            {
                EditorGUI.BeginChangeCheck();
                PWaterMeshType meshType = (PWaterMeshType)EditorGUILayout.IntPopup("Mesh Type", (int)water.MeshType, meshTypeLabels, meshTypes);
                PPlaneMeshPattern planePattern = (PPlaneMeshPattern)EditorGUILayout.EnumPopup("Pattern", water.PlanePattern);
                int meshResolution = EditorGUILayout.DelayedIntField("Resolution", water.MeshResolution);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water, "Change water mesh settings");
                    water.MeshType = meshType;
                    water.PlanePattern = planePattern;
                    water.MeshResolution = meshResolution;

                    water.GenerateMesh();
                    water.ReCalculateBounds();
                }

                EditorGUI.BeginChangeCheck();
                float meshNoise = EditorGUILayout.FloatField("Noise", water.MeshNoise);
                Vector2 tileSize = PEditorCommon.InlineVector2Field("Tile Size", water.TileSize);
                bool tilesFollowMainCamera = EditorGUILayout.Toggle("Follow Main Camera", water.TilesFollowMainCamera);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water, "Change water mesh settings");
                    water.MeshNoise = meshNoise;
                    water.TileSize = tileSize;
                    water.TilesFollowMainCamera = tilesFollowMainCamera;
                    water.ReCalculateBounds();
                }
            }

            if (!isEditingTileIndices)
            {
                if (GUILayout.Button("Edit Tiles"))
                {
                    isEditingTileIndices = true;
                }
            }
            else
            {
                EditorGUILayout.LabelField("Edit water tiles in Scene View.", PEditorCommon.WordWrapItalicLabel);
                if (GUILayout.Button("End Editing Tiles"))
                {
                    isEditingTileIndices = false;
                }
            }
        }

        private void DrawAreaMeshGUI()
        {
            if (!isEditingAreaMesh)
            {
                EditorGUI.BeginChangeCheck();
                PWaterMeshType meshType = (PWaterMeshType)EditorGUILayout.IntPopup("Mesh Type", (int)water.MeshType, meshTypeLabels, meshTypes);
                int meshResolution = EditorGUILayout.DelayedIntField("Resolution", water.MeshResolution);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water, "Change water mesh settings");
                    water.MeshType = meshType;
                    water.MeshResolution = meshResolution;
                    water.GenerateMesh();
                    water.ReCalculateBounds();
                }

                EditorGUI.BeginChangeCheck();
                float meshNoise = EditorGUILayout.FloatField("Noise", water.MeshNoise);
                if (EditorGUI.EndChangeCheck())
                {

                    Undo.RecordObject(water, "Change water mesh settings");
                    water.MeshNoise = meshNoise;
                }
            }

            if (!isEditingAreaMesh)
            {
                if (GUILayout.Button("Edit Area"))
                {
                    isEditingAreaMesh = true;
                }
            }
            else
            {
                if (GUILayout.Button("End Editing Area"))
                {
                    isEditingAreaMesh = false;
                    water.GenerateAreaMesh();
                    water.ReCalculateBounds();
                }
            }
        }

        private void DrawSplineMeshGUI()
        {
            if (!isEditingSplineMesh)
            {
                EditorGUI.BeginChangeCheck();
                PWaterMeshType meshType = (PWaterMeshType)EditorGUILayout.IntPopup("Mesh Type", (int)water.MeshType, meshTypeLabels, meshTypes);
                int splineResolutionX = EditorGUILayout.DelayedIntField("Resolution X", water.SplineResolutionX);
                int splineResolutionY = EditorGUILayout.DelayedIntField("Resolution Y", water.SplineResolutionY);
                float splineWidth = EditorGUILayout.DelayedFloatField("Width", water.SplineWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water, "Change water mesh settings");
                    water.MeshType = meshType;
                    water.SplineResolutionX = splineResolutionX;
                    water.SplineResolutionY = splineResolutionY;
                    water.SplineWidth = splineWidth;
                    water.GenerateMesh();
                    water.ReCalculateBounds();
                }
            }

            if (!isEditingSplineMesh)
            {
                if (GUILayout.Button("Edit Spline"))
                {
                    isEditingSplineMesh = true;
                }
            }
            else
            {
                PSplineToolConfig.Instance.RaycastLayer = EditorGUILayout.LayerField("Raycast Layer", PSplineToolConfig.Instance.RaycastLayer);
                PSplineToolConfig.Instance.YOffset = EditorGUILayout.FloatField("Y Offset", PSplineToolConfig.Instance.YOffset);
                EditorGUI.BeginChangeCheck();
                PSplineToolConfig.Instance.AutoTangent = EditorGUILayout.Toggle("Auto Tangent", PSplineToolConfig.Instance.AutoTangent);
                if (EditorGUI.EndChangeCheck() && PSplineToolConfig.Instance.AutoTangent)
                {
                    water.Spline.SmoothTangents();
                    water.GenerateSplineMesh();
                    water.ReCalculateBounds();
                }
                EditorUtility.SetDirty(PSplineToolConfig.Instance);
                DrawSelectedAnchorGUI();
                DrawSelectedSegmentGUI();

                if (GUILayout.Button("Smooth Tangents"))
                {
                    water.Spline.SmoothTangents();
                    water.GenerateSplineMesh();
                    water.ReCalculateBounds();
                }
                if (GUILayout.Button("Pivot To Spline Center"))
                {
                    PSplineUtilities.WaterPivotToSplineCenter(water);
                    water.GenerateSplineMesh();
                    water.ReCalculateBounds();
                }
                if (GUILayout.Button("End Editing Spline"))
                {
                    isEditingSplineMesh = false;
                    water.GenerateSplineMesh();
                    water.ReCalculateBounds();
                }
            }
        }

        private void DrawSelectedAnchorGUI()
        {
            int anchorIndex = splineEditingGUIDrawer.selectedAnchorIndex;
            if (anchorIndex < 0 ||
                anchorIndex >= water.Spline.Anchors.Count)
                return;
            string label = "Selected Anchor";
            string id = "poseidon-selected-anchor";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUI.indentLevel -= 1;
                PSplineAnchor a = water.Spline.Anchors[anchorIndex];
                EditorGUI.BeginChangeCheck();
                a.Position = PEditorCommon.InlineVector3Field("Position", a.Position);
                a.Rotation = Quaternion.Euler(PEditorCommon.InlineVector3Field("Rotation", a.Rotation.eulerAngles));
                a.Scale = PEditorCommon.InlineVector3Field("Scale", a.Scale);
                water.Spline.Anchors[anchorIndex] = a;
                if (EditorGUI.EndChangeCheck())
                {
                    if (PSplineToolConfig.Instance.AutoTangent)
                    {
                        int[] segmentIndices = water.Spline.SmoothTangents(anchorIndex);
                        water.GenerateSplineMeshAtSegments(segmentIndices);
                    }
                    else
                    {
                        List<int> segmentIndices = water.Spline.FindSegments(anchorIndex);
                        water.GenerateSplineMeshAtSegments(segmentIndices);
                    }
                }
                EditorGUI.indentLevel += 1;
            });
        }

        private void DrawSelectedSegmentGUI()
        {
            int segmentIndex = splineEditingGUIDrawer.selectedSegmentIndex;
            if (segmentIndex < 0 ||
                segmentIndex >= water.Spline.Anchors.Count)
                return;
            string label = "Selected Segment";
            string id = "poseidon-selected-segment";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUI.indentLevel -= 1;
                EditorGUI.BeginChangeCheck();
                PSplineSegment s = water.Spline.Segments[segmentIndex];
                GUI.enabled = !PSplineToolConfig.Instance.AutoTangent;
                s.StartTangent = PEditorCommon.InlineVector3Field("Start Tangent", s.StartTangent);
                s.EndTangent = PEditorCommon.InlineVector3Field("End Tangent", s.EndTangent);
                GUI.enabled = true;
                s.ResolutionMultiplierY = EditorGUILayout.Slider("Resolution Multiplier Y", s.ResolutionMultiplierY, 0f, 2f);
                water.Spline.Segments[segmentIndex] = s;
                if (EditorGUI.EndChangeCheck())
                {
                    water.GenerateSplineMeshAtSegment(segmentIndex);
                }
                EditorGUI.indentLevel += 1;
            });
        }

        private void DrawCustomMeshGUI()
        {
            EditorGUI.BeginChangeCheck();
            PWaterMeshType meshType = (PWaterMeshType)EditorGUILayout.IntPopup("Mesh Type", (int)water.MeshType, meshTypeLabels, meshTypes);
            Mesh sourceMesh = EditorGUILayout.ObjectField("Source Mesh", water.SourceMesh, typeof(Mesh), false) as Mesh;
            float meshNoise = EditorGUILayout.FloatField("Noise", water.MeshNoise);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(water, "Change water mesh settings");
                water.MeshType = meshType;
                water.SourceMesh = sourceMesh;
                water.MeshNoise = meshNoise;
                if (water.SourceMesh != null)
                {
                    water.GenerateMesh();
                }
                water.ReCalculateBounds();
            }
        }

        private void DrawRenderingSettingsGUI()
        {
            string label = "Rendering";
            string id = "water-profile-general";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                if (water.MaterialToRender != null)
                {
                    water.MaterialToRender.name = water.MaterialToRender.shader.name;
                }
                if (water.MaterialBackFace != null)
                {
                    water.MaterialBackFace.name = water.MaterialBackFace.shader.name;
                }
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Material", water.MaterialToRender, typeof(Material), false);
                if (water.MeshType == PWaterMeshType.TileablePlane && water.ShouldRenderBackface)
                {
                    EditorGUILayout.ObjectField("Material Back Face", water.MaterialBackFace, typeof(Material), false);
                }
                GUI.enabled = true;

                EditorGUI.BeginChangeCheck();
                PWaterProfile profile = water.Profile;
                int renderQueueIndex = EditorGUILayout.IntField("Queue Index", water.Profile.RenderQueueIndex);
                PLightingModel lightingModel = (PLightingModel)EditorGUILayout.EnumPopup("Light Model", water.Profile.LightingModel);
                bool useFlatLighting = EditorGUILayout.Toggle("Flat Lighting", water.Profile.UseFlatLighting);
                bool renderBackFace = water.ShouldRenderBackface;
                if (water.MeshType == PWaterMeshType.TileablePlane)
                {
                    renderBackFace = EditorGUILayout.Toggle("Render Back Face", water.ShouldRenderBackface);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water, "Change water rendering settings");
                    Undo.RecordObject(water.Profile, "Change water rendering settings");
                    water.Profile.RenderQueueIndex = renderQueueIndex;
                    water.Profile.LightingModel = lightingModel;
                    water.Profile.UseFlatLighting = useFlatLighting;
                    water.ShouldRenderBackface = renderBackFace;
                }
            });
        }

        private void DrawTimeSettingsGUI()
        {
            string label = "Time";
            string id = "water-time";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUI.BeginChangeCheck();
                PTimeMode timeMode = (PTimeMode)EditorGUILayout.EnumPopup("Time Mode", water.TimeMode);
                float manualTime = (float)water.ManualTimeSeconds;
                if (timeMode == PTimeMode.Auto)
                {
                    EditorGUILayout.LabelField("Time", water.GetTimeParam().ToString());
                }
                else
                {
                    manualTime = EditorGUILayout.FloatField("Time", (float)water.ManualTimeSeconds);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water, "Change water time settings");
                    water.TimeMode = timeMode;
                    water.ManualTimeSeconds = manualTime;
                }
            });
        }

        private void DrawColorsSettingsGUI()
        {
            string label = "Colors";
            string id = "water-profile-colors";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUI.BeginChangeCheck();
                Color color = EditorGUILayout.ColorField("Color", water.Profile.Color);
                Color depthColor = water.Profile.DepthColor;
                Color specColor = water.Profile.SpecColor;
                float smoothness = water.Profile.Smoothness;
                if (water.Profile.EnableLightAbsorption)
                {
                    depthColor = EditorGUILayout.ColorField("Depth Color", water.Profile.DepthColor);
                }
                if (water.Profile.LightingModel == PLightingModel.PhysicalBased || water.Profile.LightingModel == PLightingModel.BlinnPhong)
                {
                    specColor = EditorGUILayout.ColorField(new GUIContent("Specular Color"), water.Profile.SpecColor, true, false, true);
                    smoothness = EditorGUILayout.Slider("Smoothness", water.Profile.Smoothness, 0f, 1f);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water.Profile, "Change water color settings");
                    water.Profile.Color = color;
                    water.Profile.DepthColor = depthColor;
                    water.Profile.SpecColor = specColor;
                    water.Profile.Smoothness = smoothness;
                }
            });
        }

        private void DrawFresnelSettingsGUI()
        {
            string label = "Fresnel";
            string id = "water-profile-fresnel";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUI.BeginChangeCheck();
                float fresnelStrength = EditorGUILayout.Slider("Strength", water.Profile.FresnelStrength, 0f, 10f);
                float fresnelBias = EditorGUILayout.Slider("Bias", water.Profile.FresnelBias, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water.Profile, "Change water fresnel settings");
                    water.Profile.FresnelStrength = fresnelStrength;
                    water.Profile.FresnelBias = fresnelBias;
                }
            });
        }

        private void DrawLightAbsorbtionSettingsGUI()
        {
            string label = "Light Absorption";
            string id = "water-profile-absorption";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUI.BeginChangeCheck();
                bool enableLightAbsorption = EditorGUILayout.Toggle("Enable", water.Profile.EnableLightAbsorption);
                Color depthColor = water.Profile.DepthColor;
                float maxDepth = water.Profile.MaxDepth;
                if (enableLightAbsorption)
                {
                    depthColor = EditorGUILayout.ColorField("Depth Color", water.Profile.DepthColor);
                    maxDepth = EditorGUILayout.FloatField("Max Depth", water.Profile.MaxDepth);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water.Profile, "Change water light absorption settings");
                    water.Profile.EnableLightAbsorption = enableLightAbsorption;
                    water.Profile.DepthColor = depthColor;
                    water.Profile.MaxDepth = maxDepth;
                }
            });
        }

        private void DrawFoamSettingsGUI()
        {
            string label = "Foam";
            string id = "water-profile-foam";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUI.BeginChangeCheck();
                bool enableFoam = EditorGUILayout.Toggle("Enable", water.Profile.EnableFoam);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water.Profile, "Change water foam settings");
                    water.Profile.EnableFoam = enableFoam;
                }

                if (enableFoam)
                {
                    EditorGUI.BeginChangeCheck();
                    bool enableFoamHQ = EditorGUILayout.Toggle("High Quality", water.Profile.EnableFoamHQ);
                    float foamNoiseScaleHQ = water.Profile.FoamNoiseScaleHQ;
                    float foamNoiseSpeedHQ = water.Profile.FoamNoiseSpeedHQ;
                    if (enableFoamHQ)
                    {
                        foamNoiseScaleHQ = EditorGUILayout.FloatField("Scale", water.Profile.FoamNoiseScaleHQ);
                        foamNoiseSpeedHQ = EditorGUILayout.FloatField("Speed", water.Profile.FoamNoiseSpeedHQ);
                    }
                    Color foamColor = EditorGUILayout.ColorField(new GUIContent("Color"), water.Profile.FoamColor, true, true, true);

                    PEditorCommon.Header("Shoreline");
                    float foamDistance = EditorGUILayout.FloatField("Distance", water.Profile.FoamDistance);
                    float shorelineFoamStrength = EditorGUILayout.Slider("Strength", water.Profile.ShorelineFoamStrength, 0f, 1f);

                    float crestMaxDepth = water.Profile.CrestMaxDepth;
                    float crestFoamStrength = water.Profile.CrestFoamStrength;
                    if (water.Profile.EnableWave)
                    {
                        PEditorCommon.Header("Crest");
                        crestMaxDepth = EditorGUILayout.FloatField("Max Depth", water.Profile.CrestMaxDepth);
                        crestFoamStrength = EditorGUILayout.Slider("Strength", water.Profile.CrestFoamStrength, 0f, 1f);
                    }

                    float slopeFoamDistance = water.Profile.SlopeFoamDistance;
                    float slopeFoamFlowSpeed = water.Profile.SlopeFoamFlowSpeed;
                    float slopeFoamStrength = water.Profile.SlopeFoamStrength;
                    if (water.MeshType == PWaterMeshType.Spline)
                    {
                        PEditorCommon.Header("Slope");
                        slopeFoamDistance = EditorGUILayout.FloatField("Distance", water.Profile.SlopeFoamDistance);
                        slopeFoamFlowSpeed = EditorGUILayout.FloatField("Flow Speed", water.Profile.SlopeFoamFlowSpeed);
                        slopeFoamStrength = EditorGUILayout.Slider("Strength", water.Profile.SlopeFoamStrength, 0f, 1f);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(water.Profile, "Change water foam settings");
                        water.Profile.EnableFoamHQ = enableFoamHQ;
                        water.Profile.FoamNoiseScaleHQ = foamNoiseScaleHQ;
                        water.Profile.FoamNoiseSpeedHQ = foamNoiseSpeedHQ;
                        water.Profile.FoamColor = foamColor;
                        water.Profile.FoamDistance = foamDistance;
                        water.Profile.ShorelineFoamStrength = shorelineFoamStrength;
                        water.Profile.CrestMaxDepth = crestMaxDepth;
                        water.Profile.CrestFoamStrength = crestFoamStrength;
                        water.Profile.SlopeFoamDistance = slopeFoamDistance;
                        water.Profile.SlopeFoamFlowSpeed = slopeFoamFlowSpeed;
                        water.Profile.SlopeFoamStrength = slopeFoamStrength;
                    }
                }
            });
        }

        private void DrawRippleSettingsGUI()
        {
            string label = "Ripple";
            string id = "water-profile-ripple";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUI.BeginChangeCheck();
                float rippleHeight = EditorGUILayout.Slider("Height", water.Profile.RippleHeight, 0f, 1f);
                float rippleNoiseScale = EditorGUILayout.FloatField("Scale", water.Profile.RippleNoiseScale);
                float rippleSpeed = EditorGUILayout.FloatField("Speed", water.Profile.RippleSpeed);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water.Profile, "Change water ripple settings");
                    water.Profile.RippleHeight = rippleHeight;
                    water.Profile.RippleNoiseScale = rippleNoiseScale;
                    water.Profile.RippleSpeed = rippleSpeed;
                }
            });
        }

        private void DrawWaveSettingsGUI()
        {
            string label = "Wave";
            string id = "water-profile-wave";

            PEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUI.BeginChangeCheck();
                bool enableWave = EditorGUILayout.Toggle("Enable", water.Profile.EnableWave);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water.Profile, "Change water wave settings");
                    water.Profile.EnableWave = enableWave;
                }

                if (enableWave)
                {
                    EditorGUI.BeginChangeCheck();
                    float waveDirection = EditorGUILayout.Slider("Direction", water.Profile.WaveDirection, 0f, 360f);
                    float waveSpeed = EditorGUILayout.FloatField("Speed", water.Profile.WaveSpeed);
                    float waveHeight = EditorGUILayout.FloatField("Height", water.Profile.WaveHeight);
                    float waveLength = EditorGUILayout.FloatField("Length", water.Profile.WaveLength);
                    float waveSteepness = EditorGUILayout.Slider("Steepness", water.Profile.WaveSteepness, 0f, 1f);
                    float waveDeform = EditorGUILayout.Slider("Deform", water.Profile.WaveDeform, 0f, 1f);
                    bool useWaveMask = EditorGUILayout.Toggle("Use Mask", water.UseWaveMask);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(water, "Change water wave settings");
                        Undo.RecordObject(water.Profile, "Change water wave settings");
                        water.Profile.WaveDirection = waveDirection;
                        water.Profile.WaveSpeed = waveSpeed;
                        water.Profile.WaveHeight = waveHeight;
                        water.Profile.WaveLength = waveLength;
                        water.Profile.WaveSteepness = waveSteepness;
                        water.Profile.WaveDeform = waveDeform;
                        water.UseWaveMask = useWaveMask;
                    }

                    if (useWaveMask)
                    {
                        EditorGUI.BeginChangeCheck();
                        waveMaskVisMode = (PWaveMaskVisualizationMode)EditorGUILayout.EnumPopup("Visualization", waveMaskVisMode);
                        EditorGUIUtility.wideMode = true;
                        Rect waveMaskBounds = EditorGUILayout.RectField("Bounds", water.WaveMaskBounds);
                        EditorGUIUtility.wideMode = false;
                        Texture2D waveMask = PEditorCommon.InlineTexture2DField("Mask", water.WaveMask, -1);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(water, "Change water wave settings");
                            water.WaveMaskBounds = waveMaskBounds;
                            water.WaveMask = waveMask;
                        }
                    }
                }
            });
        }

        private void DrawReflectionSettingsGUI()
        {
            if (water.MeshType == PWaterMeshType.Spline)
                return;

            bool stereoEnable = false;
            if (Camera.main != null)
            {
                stereoEnable = Camera.main.stereoEnabled;
            }

            string label = "Reflection" + (stereoEnable ? " (Not support for VR)" : "");
            string id = "water-profile-reflection";

            GUI.enabled = !stereoEnable;
            PEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUI.BeginChangeCheck();
                bool enableReflection = EditorGUILayout.Toggle("Enable", water.Profile.EnableReflection);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water.Profile, "Change water reflection settings");
                    water.Profile.EnableReflection = enableReflection;
                }

                if (enableReflection)
                {
                    EditorGUI.BeginChangeCheck();
                    bool reflectCustomSkybox = EditorGUILayout.Toggle("Custom Skybox", water.Profile.ReflectCustomSkybox);
                    bool enableReflectionPixelLight = EditorGUILayout.Toggle("Pixel Light", water.Profile.EnableReflectionPixelLight);
                    float reflectionClipPlaneOffset = EditorGUILayout.FloatField("Clip Plane Offset", water.Profile.ReflectionClipPlaneOffset);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(water.Profile, "Change water reflection settings");
                        water.Profile.ReflectCustomSkybox = reflectCustomSkybox;
                        water.Profile.EnableReflectionPixelLight = enableReflectionPixelLight;
                        water.Profile.ReflectionClipPlaneOffset = reflectionClipPlaneOffset;
                    }

                    SerializedObject profileSO = new SerializedObject(water.Profile);
                    SerializedProperty reflectionLayerProps = profileSO.FindProperty("reflectionLayers");
                    if (reflectionLayerProps != null)
                    {
                        EditorGUILayout.PropertyField(reflectionLayerProps);
                    }
                    profileSO.ApplyModifiedProperties();
                    reflectionLayerProps.Dispose();
                    profileSO.Dispose();

                    EditorGUI.BeginChangeCheck();
                    int reflectionTextureResolution = EditorGUILayout.IntPopup("Resolution", water.Profile.ReflectionTextureResolution, renderTextureSizeLabels, renderTextureSizes);
                    float reflectionDistortionStrength = EditorGUILayout.FloatField("Distortion", water.Profile.ReflectionDistortionStrength);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(water.Profile, "Change water reflection settings");
                        water.Profile.ReflectionTextureResolution = reflectionTextureResolution;
                        water.Profile.ReflectionDistortionStrength = reflectionDistortionStrength;
                    }
                }
            });
            GUI.enabled = true;
        }

        private void DrawRefractionSettingsGUI()
        {
            string label = "Refraction";
            string id = "water-profile-refraction";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                if (water.MeshType != PWaterMeshType.Spline)
                {
                    EditorGUI.BeginChangeCheck();
                    bool enableRefraction = EditorGUILayout.Toggle("Enable", water.Profile.EnableRefraction);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(water.Profile, "Change water refraction settings");
                        water.Profile.EnableRefraction = enableRefraction;
                    }
                }
                if (water.Profile.EnableRefraction || water.MeshType == PWaterMeshType.Spline)
                {
                    EditorGUI.BeginChangeCheck();
                    float refractionDistortionStrength = EditorGUILayout.FloatField("Distortion", water.Profile.RefractionDistortionStrength);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(water.Profile, "Change water refraction settings");
                        water.Profile.RefractionDistortionStrength = refractionDistortionStrength;
                    }
                }
            });
        }

        private void DrawCausticSettingsGUI()
        {
            string label = "Caustic";
            string id = "water-profile-caustic";

            PEditorCommon.Foldout(label, true, id, () =>
            {
                bool valid = (water.Profile.EnableRefraction || water.MeshType == PWaterMeshType.Spline);
                if (!valid)
                {
                    EditorGUILayout.LabelField("Requires Refraction.", PEditorCommon.WordWrapItalicLabel);
                    water.Profile.EnableCaustic = false;
                }

                GUI.enabled = valid;
                EditorGUI.BeginChangeCheck();
                bool enableCaustic = EditorGUILayout.Toggle("Enable", water.Profile.EnableCaustic);
                Texture causticTexture = water.Profile.CausticTexture;
                float causticSize = water.Profile.CausticSize;
                float causticStrength = water.Profile.CausticStrength;
                float causticDistortionStrength = water.Profile.CausticDistortionStrength;
                if (water.Profile.EnableCaustic)
                {
                    causticTexture = PEditorCommon.InlineTextureField("Texture", water.Profile.CausticTexture, -1);
                    causticSize = EditorGUILayout.FloatField("Size", water.Profile.CausticSize);
                    causticStrength = EditorGUILayout.Slider("Strength", water.Profile.CausticStrength, 0f, 5f);
                    causticDistortionStrength = EditorGUILayout.FloatField("Distortion", water.Profile.CausticDistortionStrength);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(water.Profile, "Change water caustic settings");
                    water.Profile.EnableCaustic = enableCaustic;
                    water.Profile.CausticTexture = causticTexture;
                    water.Profile.CausticSize = causticSize;
                    water.Profile.CausticStrength = causticStrength;
                    water.Profile.CausticDistortionStrength = causticDistortionStrength;
                }
                GUI.enabled = true;
            });
        }

        private void DuringSceneGUI(SceneView sv)
        {
            Tools.hidden = isEditingTileIndices || isEditingAreaMesh || isEditingSplineMesh;
            if (water.MeshType == PWaterMeshType.TileablePlane && isEditingTileIndices)
            {
                DrawEditingTilesGUI();
            }

            if (water.MeshType == PWaterMeshType.Area && isEditingAreaMesh)
            {
                DrawEditingAreaMeshGUI();
            }

            if (water.MeshType == PWaterMeshType.Spline && isEditingSplineMesh)
            {
                DrawEditingSplineGUI();
            }

            if (isEditingTileIndices || isEditingAreaMesh || isEditingSplineMesh)
            {
                DrawBounds();
            }

            bool isWaveSectionExpanded = PEditorCommon.IsFoldoutExpanded("water-profile-wave");
            if (isWaveSectionExpanded && water.UseWaveMask)
            {
                DrawWaveMaskBounds();
            }
        }

        private void DrawEditingTilesGUI()
        {
            tileEditingGUIDrawer.Draw();
        }

        private void DrawEditingAreaMeshGUI()
        {
            areaEditingGUIDrawer.Draw();
        }

        private void DrawEditingSplineGUI()
        {
            splineEditingGUIDrawer.Draw();
        }

        private void DrawBounds()
        {
            if (Event.current == null)
                return;
            if (water.Profile == null)
                return;

            Vector3 center = water.transform.TransformPoint(water.Bounds.center);
            Vector3 size = water.transform.TransformVector(water.Bounds.size);
            Handles.color = Color.yellow;
            Handles.DrawWireCube(center, size);
        }

        private void DrawWaveMaskBounds()
        {
            if (Event.current == null)
                return;
            if (water.Profile == null)
                return;

            Vector2 center = water.WaveMaskBounds.center;
            Vector3 worldCenter = new Vector3(center.x, water.transform.position.y, center.y);
            Vector2 size = water.WaveMaskBounds.size;
            Vector3 worldSize = new Vector3(size.x, 0.01f, size.y);
            Handles.color = Color.cyan;
            Handles.DrawWireCube(worldCenter, worldSize);
        }

        public void DrawDebugGUI()
        {
            string label = "Debug";
            string id = "debug" + water.GetInstanceID().ToString();

            PEditorCommon.Foldout(label, false, id, () =>
            {
                Camera[] cams = water.GetComponentsInChildren<Camera>();
                for (int i = 0; i < cams.Length; ++i)
                {
                    if (!cams[i].name.StartsWith("~"))
                        continue;
                    if (cams[i].targetTexture == null)
                        continue;
                    EditorGUILayout.LabelField(cams[i].name);
                    Rect r = GUILayoutUtility.GetAspectRect(1);
                    EditorGUI.DrawPreviewTexture(r, cams[i].targetTexture);
                    EditorGUILayout.Space();
                }
            });
        }

        public override bool RequiresConstantRepaint()
        {
            return willDrawDebugGUI;
        }

        private void OnRenderCamera(Camera cam)
        {
            bool isWaveSectionExpanded = PEditorCommon.IsFoldoutExpanded("water-profile-wave");
            if (isWaveSectionExpanded && water.UseWaveMask)
            {
                if (waveMaskVisMode != PWaveMaskVisualizationMode.None)
                {
                    Vector2 center = water.WaveMaskBounds.center;
                    Vector3 worldCenter = new Vector3(center.x, water.transform.position.y, center.y);
                    Vector2 size = water.WaveMaskBounds.size;
                    Vector3 worldSize = new Vector3(size.x, size.y, 1);

                    MaskVisualizerMaterial.SetTexture("_MainTex", water.WaveMask);
                    MaskVisualizerMaterial.DisableKeyword("FLOW");
                    MaskVisualizerMaterial.DisableKeyword("CREST");
                    MaskVisualizerMaterial.DisableKeyword("HEIGHT");
                    //if (waveMaskVisMode == PWaveMaskVisualizationMode.Flow)
                    //{
                    //    MaskVisualizerMaterial.EnableKeyword("FLOW");
                    //}
                    //else 
                    if (waveMaskVisMode == PWaveMaskVisualizationMode.Crest)
                    {
                        MaskVisualizerMaterial.EnableKeyword("CREST");
                    }
                    else if (waveMaskVisMode == PWaveMaskVisualizationMode.Height)
                    {
                        MaskVisualizerMaterial.EnableKeyword("HEIGHT");
                    }

                    Graphics.DrawMesh(
                        QuadMesh,
                        Matrix4x4.TRS(worldCenter, Quaternion.Euler(90, 0, 0), worldSize),
                        MaskVisualizerMaterial,
                        0,
                        cam,
                        0,
                        null,
                        ShadowCastingMode.Off,
                        false,
                        null,
                        LightProbeUsage.Off,
                        null);
                }
            }
        }

        private void OnRenderCameraSRP(ScriptableRenderContext context, Camera cam)
        {
            OnRenderCamera(cam);
        }
    }
}
