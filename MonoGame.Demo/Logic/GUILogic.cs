using Deferred.Utilities;
using DeferredEngine.Entities;
using DeferredEngine.Pipeline;
using DeferredEngine.Pipeline.Lighting;
using DeferredEngine.Pipeline.Utilities;
using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using DeferredEngine.Rendering.PostProcessing;
using DeferredEngine.Utilities;
using HelperSuite.GUI;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;
using System.Text;

namespace DeferredEngine.Demo
{
    public class GUILogic
    {
        private DemoAssets _assets;
        public GUICanvas GuiCanvas;

        private GuiListToggleScroll _rightSideList;
        private GUIList _leftSideList;

        private GUIList _objectDescriptionList;
        private GUITextBlock _objectDescriptionName;
        private GUITextBlock _objectDescriptionPos;
        private GUITextBlockButton _objectButton1;
        private GUITextBlockToggle _objectToggle0;
        private GUITextBlockToggle _objectToggle1;
        private GUITextBlockToggle _objectToggle2;
        private GUITextBlockToggle _objectToggle3;
        private GuiSliderFloatText _objectSlider0;
        private GuiSliderFloatText _objectSlider1;
        private GuiSliderIntText _objectSlider2;
        private GUIColorPicker _objectColorPicker1;

        private GUIStyle defaultStyle;

        private GUITextBlockButton _gizmoTranslation;
        private GUITextBlockButton _gizmoRotation;
        private GUITextBlockButton _gizmoScale;
        private GizmoModes _gizmoMode;
        private GizmoModes _gizmoModePrevious;

        //Selected object
        private TransformableObject activeObject;
        private string activeObjectName;
        private Vector3 activeObjectPos;


        public void Initialize(DemoAssets assets, Camera sceneLogicCamera)
        {
            _assets = assets;

            CreateGUI(sceneLogicCamera);
        }


        /// <summary>
        /// Creates the GUI for the default editor
        /// </summary>
        /// <param name="sceneLogicCamera"></param>
        private void CreateGUI(Camera sceneLogicCamera)
        {
            GuiCanvas = new GUICanvas(Vector2.Zero, RenderingSettings.Screen.g_Resolution);

            defaultStyle = new GUIStyle(
                dimensionsStyle: new Vector2(200, 35),
                textFontStyle: _assets.MonospaceFont,
                blockColorStyle: Color.Gray,
                textColorStyle: Color.White,
                sliderColorStyle: Color.White,
                guiAlignmentStyle: GUIStyle.GUIAlignment.None,
                textAlignmentStyle: GUIStyle.TextAlignment.Left,
                textButtonAlignmentStyle: GUIStyle.TextAlignment.Center,
                textBorderStyle: new Vector2(10, 1),
                parentDimensionsStyle: GuiCanvas.Dimensions);

            //Editor gizmo control!
            GuiCanvas.AddElement(_leftSideList = new GUIList(Vector2.Zero, defaultStyle));

            _leftSideList.AddElement(_gizmoTranslation = new GUITextBlockButton(defaultStyle, "Translate (T)")
            {
                ButtonObject = this,
                ButtonMethod = GetType().GetMethod("ChangeGizmoMode"),
                ButtonMethodArgs = new object[] { GizmoModes.Translation },
            });
            _leftSideList.AddElement(_gizmoRotation = new GUITextBlockButton(defaultStyle, "Rotate (R)")
            {
                ButtonObject = this,
                ButtonMethod = GetType().GetMethod("ChangeGizmoMode"),
                ButtonMethodArgs = new object[] { GizmoModes.Rotation },
            });
            _leftSideList.AddElement(_gizmoScale = new GUITextBlockButton(defaultStyle, "Scale (Z)")
            {
                ButtonObject = this,
                ButtonMethod = GetType().GetMethod("ChangeGizmoMode"),
                ButtonMethodArgs = new object[] { GizmoModes.Scale },
            });
            _leftSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Local: ")
            {
                ToggleField = typeof(RenderingSettings).GetField(nameof(RenderingSettings.e_LocalTransformation)),
                Toggle = RenderingSettings.e_LocalTransformation
            });
            _leftSideList.Alignment = GUIStyle.GUIAlignment.BottomLeft;

            ChangeGizmoMode(GizmoModes.Translation);

            //Editor options
            GuiCanvas.AddElement(_rightSideList = new GuiListToggleScroll(new Vector2(-20, 0), defaultStyle));

            GUITextBlock helperText = new GUITextBlock(new Vector2(0, 100), new Vector2(300, 200), CreateHelperText(), defaultStyle.TextFontStyle, new Color(Color.DimGray, 0.2f), Color.White, GUIStyle.TextAlignment.Left, new Vector2(10, 1)) { IsHidden = true };
            GuiCanvas.AddElement(helperText);

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Selection")
            {
                ToggleProperty = typeof(RenderingSettings).GetProperty(nameof(RenderingSettings.EnableSelection)),
                Toggle = RenderingSettings.EnableSelection
            });

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Draw Mesh Outl.")
            {
                ToggleField = typeof(IdAndOutlineRenderModule).GetField("e_DrawOutlines"),
                Toggle = IdAndOutlineRenderModule.e_DrawOutlines
            });

            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Profiler")
            {
                ToggleProperty = PipelineProfiler.ModuleEnabled.GetValuePropertyInfo(),
                ToggleObject = PipelineProfiler.ModuleEnabled,
                Toggle = PipelineProfiler.ModuleEnabled
            });
            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Decals")
            {
                ToggleField = typeof(DecalRenderModule).GetField("g_EnableDecals"),
                Toggle = DecalRenderModule.g_EnableDecals
            });
            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Forward")
            {
                ToggleProperty = ForwardPipelineModule.ModuleEnabled.GetValuePropertyInfo(),
                ToggleObject = ForwardPipelineModule.ModuleEnabled,
                Toggle = ForwardPipelineModule.ModuleEnabled
            });
            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable CPU Cull")
            {
                ToggleProperty = DynamicMeshBatcher.CPUCullingEnabled.GetValuePropertyInfo(),
                ToggleObject = DynamicMeshBatcher.CPUCullingEnabled,
                Toggle = DynamicMeshBatcher.CPUCullingEnabled
            });
            _rightSideList.AddElement(new GuiSliderFloatText(defaultStyle, 16.0f, 1024.0f, 2, "Far Clip: ")
            {
                SliderProperty = RenderingSettings.Screen.g_FarClip.GetValuePropertyInfo(),
                SliderObject = RenderingSettings.Screen.g_FarClip,
                SliderValue = RenderingSettings.Screen.g_FarClip,
            });

            _rightSideList.AddElement(new GuiSliderFloatText(defaultStyle, 0.1f, 3/*(float) (Math.PI - 0.1)*/, 2, "Field Of View: ")
            {
                SliderObject = sceneLogicCamera,
                SliderProperty = typeof(Camera).GetProperty("FieldOfView"),
                SliderValue = sceneLogicCamera.FieldOfView
            });


            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Show Controls")
            {
                ToggleProperty = typeof(GUITextBlock).GetProperty("IsVisible"),
                ToggleObject = helperText,
                Toggle = helperText.IsVisible
            });
            _rightSideList.AddElement(new GUITextBlockToggle(defaultStyle, "Default Material")
            {
                ToggleField = typeof(RenderingSettings).GetField("d_DefaultMaterial"),
                Toggle = RenderingSettings.d_DefaultMaterial
            });

            //_rightSideList.AddElement(new GuiDropList(defaultStyle, "Show: ")
            //{
            //});

            _rightSideList.AddElement(new GUITextBlock(defaultStyle, "Selection") { BlockColor = Color.DimGray, Dimensions = new Vector2(200, 10), TextAlignment = GUIStyle.TextAlignment.Center });

            GuiListToggle _selectionList = new GuiListToggle(Vector2.Zero, defaultStyle);
            _objectDescriptionList = new GUIList(Vector2.Zero, defaultStyle);

            _objectDescriptionList.AddElement(_objectDescriptionName = new GUITextBlock(defaultStyle, "objDescName"));
            _objectDescriptionList.AddElement(_objectDescriptionPos = new GUITextBlock(defaultStyle, "objDescName"));
            _objectDescriptionList.AddElement(_objectButton1 = new GUITextBlockButton(defaultStyle, "objButton1") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectToggle0 = new GUITextBlockToggle(defaultStyle, "objToggle0") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectToggle1 = new GUITextBlockToggle(defaultStyle, "objToggle1") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectToggle2 = new GUITextBlockToggle(defaultStyle, "objToggle2") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectToggle3 = new GUITextBlockToggle(defaultStyle, "objToggle3") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectSlider0 = new GuiSliderFloatText(defaultStyle, 0, 1, 2, "objToggle1") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectSlider1 = new GuiSliderFloatText(defaultStyle, 0, 1, 2, "objToggle2") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectSlider2 = new GuiSliderIntText(defaultStyle, 0, 10, 1, "objToggle3") { IsHidden = true });
            _objectDescriptionList.AddElement(_objectColorPicker1 = new GUIColorPicker(defaultStyle) { IsHidden = true });

            _selectionList.AddElement(_objectDescriptionList);
            _rightSideList.AddElement(_selectionList);

            /////////////////////////////////////////////////////////////////
            //Options
            /////////////////////////////////////////////////////////////////

            _rightSideList.AddElement(new GUITextBlock(defaultStyle, "Options") { BlockColor = Color.DimGray, Dimensions = new Vector2(200, 10), TextAlignment = GUIStyle.TextAlignment.Center });

            GuiListToggle optionList = new GuiListToggle(Vector2.Zero, defaultStyle);
            _rightSideList.AddElement(optionList);

            /////////////////////////////////////////////////////////////////
            //SDF
            /////////////////////////////////////////////////////////////////
            /// 
            optionList.AddElement(new GUITextBlock(Vector2.Zero, new Vector2(200, 10), "SDF",
                defaultStyle.TextFontStyle, Color.DarkSlateGray, Color.White, GUIStyle.TextAlignment.Center,
                Vector2.Zero));

            GuiListToggle sdfList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
            optionList.AddElement(sdfList);

            sdfList.AddElement(new GUITextBlockToggle(defaultStyle, "Draw SDF")
            {
                ToggleField = typeof(RenderingSettings.SDF).GetField(nameof(RenderingSettings.SDF.DrawDistance)),
                Toggle = RenderingSettings.SDF.DrawDistance
            });

            sdfList.AddElement(new GUITextBlockToggle(defaultStyle, "Draw SDF volume")
            {
                ToggleField = typeof(RenderingSettings.SDF).GetField(nameof(RenderingSettings.SDF.DrawVolume)),
                Toggle = RenderingSettings.SDF.DrawVolume
            });

            /////////////////////////////////////////////////////////////////
            //Post Processing
            /////////////////////////////////////////////////////////////////

            optionList.AddElement(new GUITextBlock(defaultStyle, "PostProcessing") { BlockColor = Color.DarkSlateGray, Dimensions = new Vector2(200, 10), TextAlignment = GUIStyle.TextAlignment.Center });

            GuiListToggle postprocessingList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
            optionList.AddElement(postprocessingList);

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "Post Processing")
            {
                ToggleProperty = PostProcessingFx.ModuleEnabled.GetValuePropertyInfo(),
                ToggleObject = PostProcessingFx.ModuleEnabled,
                Toggle = PostProcessingFx.ModuleEnabled
            });

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "Color Grading")
            {
                ToggleProperty = ColorGradingFx.ModuleEnabled.GetValuePropertyInfo(),
                ToggleObject = ColorGradingFx.ModuleEnabled,
                Toggle = ColorGradingFx.ModuleEnabled
            });

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "TAA ")
            {
                ToggleProperty = TemporalAAFx.ModuleEnabled.GetValuePropertyInfo(),
                ToggleObject = TemporalAAFx.ModuleEnabled,
                Toggle = TemporalAAFx.ModuleEnabled
            });

            postprocessingList.AddElement(new GUITextBlockToggle(defaultStyle, "TAA Tonemap")
            {
                ToggleProperty = TemporalAAFx.ModuleTonemapEnabled.GetValuePropertyInfo(),
                ToggleObject = TemporalAAFx.ModuleTonemapEnabled,
                Toggle = TemporalAAFx.ModuleTonemapEnabled
            });


            /////////////////////////////////////////////////////////////////
            //SSR
            /////////////////////////////////////////////////////////////////

            optionList.AddElement(new GUITextBlock(Vector2.Zero, new Vector2(200, 10), "Screen Space Reflections",
                defaultStyle.TextFontStyle, Color.DarkSlateGray, Color.White, GUIStyle.TextAlignment.Center,
                Vector2.Zero));

            GuiListToggle ssrList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
            optionList.AddElement(ssrList);

            ssrList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable SSR")
            {
                ToggleProperty = SSReflectionFx.ModuleEnabled.GetValuePropertyInfo(),
                ToggleObject = SSReflectionFx.ModuleEnabled,
                Toggle = SSReflectionFx.ModuleEnabled
            });

            ssrList.AddElement(new GUITextBlockToggle(defaultStyle, "Stochastic distr.")
            {
                ToggleProperty = SSReflectionFx.g_UseTaa.GetValuePropertyInfo(),
                ToggleObject = SSReflectionFx.g_UseTaa,
                Toggle = SSReflectionFx.g_UseTaa
            });

            ssrList.AddElement(new GUITextBlockToggle(defaultStyle, "Temporal Noise")
            {
                ToggleProperty = SSReflectionFx.g_Noise.GetValuePropertyInfo(),
                ToggleObject = SSReflectionFx.g_Noise,
                Toggle = SSReflectionFx.g_Noise
            });


            ssrList.AddElement(new GUITextBlockToggle(defaultStyle, "Firefly Reduction")
            {
                ToggleProperty = typeof(SSReflectionFx).GetProperty(nameof(SSReflectionFx.g_FireflyReduction)),
                Toggle = SSReflectionFx.g_FireflyReduction
            });

            ssrList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 5, 2, "Firefly Threshold ")
            {
                SliderProperty = typeof(SSReflectionFx).GetProperty(nameof(SSReflectionFx.g_FireflyThreshold)),
                SliderValue = SSReflectionFx.g_FireflyThreshold
            });

            ssrList.AddElement(new GuiSliderIntText(defaultStyle, 1, 100, 1, "Samples: ")
            {
                SliderProperty = SSReflectionFx.g_Samples.GetValuePropertyInfo(),
                SliderObject = SSReflectionFx.g_Samples,
                SliderValue = SSReflectionFx.g_Samples
            }); ;

            ssrList.AddElement(new GuiSliderIntText(defaultStyle, 1, 100, 1, "Search Samples: ")
            {
                SliderProperty = SSReflectionFx.g_RefinementSamples.GetValuePropertyInfo(),
                SliderObject = SSReflectionFx.g_RefinementSamples,
                SliderValue = SSReflectionFx.g_RefinementSamples
            });

            /////////////////////////////////////////////////////////////////
            //SSAO
            /////////////////////////////////////////////////////////////////
            /// 
            optionList.AddElement(new GUITextBlock(Vector2.Zero, new Vector2(200, 10), "Ambient Occlusion",
                defaultStyle.TextFontStyle, Color.DarkSlateGray, Color.White, GUIStyle.TextAlignment.Center,
                Vector2.Zero));

            GuiListToggle ssaoList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
            optionList.AddElement(ssaoList);

            ssaoList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable SSAO")
            {
                ToggleProperty = SSAmbientOcclustionFx.ModuleEnabled.GetValuePropertyInfo(),
                ToggleObject = SSAmbientOcclustionFx.ModuleEnabled,
                Toggle = SSAmbientOcclustionFx.ModuleEnabled
            });

            ssaoList.AddElement(new GUITextBlockToggle(defaultStyle, "SSAO Blur: ")
            {
                ToggleProperty = SSAmbientOcclustionFx.ModuleEnableBlur.GetValuePropertyInfo(),
                ToggleObject = SSAmbientOcclustionFx.ModuleEnableBlur,
                Toggle = SSAmbientOcclustionFx.ModuleEnableBlur
            });

            ssaoList.AddElement(new GuiSliderIntText(defaultStyle, 1, 32, 1, "SSAO Samples: ")
            {
                SliderProperty = SSAmbientOcclustionFx.ModuleSamples.GetValuePropertyInfo(),
                SliderObject = SSAmbientOcclustionFx.ModuleSamples,
                SliderValue = SSAmbientOcclustionFx.ModuleSamples
            });

            ssaoList.AddElement(new GuiSliderFloatText(defaultStyle, 1, 100, 2, "Sample Radius: ")
            {
                SliderProperty = SSAmbientOcclustionFx.ModuleRadius.GetValuePropertyInfo(),
                SliderObject = SSAmbientOcclustionFx.ModuleRadius,
                SliderValue = SSAmbientOcclustionFx.ModuleRadius
            });

            ssaoList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 4, 1, "SSAO Strength: ")
            {
                SliderProperty = SSAmbientOcclustionFx.ModuleStrength.GetValuePropertyInfo(),
                SliderObject = SSAmbientOcclustionFx.ModuleStrength,
                SliderValue = SSAmbientOcclustionFx.ModuleStrength
            });

            /////////////////////////////////////////////////////////////////
            //Bloom
            /////////////////////////////////////////////////////////////////
            /// 
            optionList.AddElement(new GUITextBlock(Vector2.Zero, new Vector2(200, 10), "Bloom",
                defaultStyle.TextFontStyle, Color.DarkSlateGray, Color.White, GUIStyle.TextAlignment.Center,
                Vector2.Zero));

            GuiListToggle bloomList = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
            optionList.AddElement(bloomList);

            bloomList.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Bloom")
            {
                ToggleProperty = BloomFx.ModuleEnabled.GetValuePropertyInfo(),
                ToggleObject = BloomFx.ModuleEnabled,
                Toggle = BloomFx.ModuleEnabled
            });

            bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 1, 3, "Threshold: ")
            {
                SliderProperty = BloomFx.ModuleThreshold.GetValuePropertyInfo(),
                SliderObject = BloomFx.ModuleThreshold,
                SliderValue = BloomFx.ModuleThreshold,
            });

            // ToDo: @tpott: UI Controls: Reintroduce UI for bloom values
            //for (int i = 0; i < 5; i++)
            //{
            //    bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP0 Radius: ")
            //    {
            //        SliderField = typeof(RenderingSettings.Bloom).GetField("Radius1"),
            //        SliderValue = RenderingSettings.Bloom.Radius[i]
            //    });

            //    bloomList.AddElement(new GuiSliderFloatText(defaultStyle, 0, 8, 1, "MIP0 Strength: ")
            //    {
            //        SliderField = typeof(RenderingSettings.Bloom).GetField("Strength1"),
            //        SliderValue = RenderingSettings.Bloom.Strength1
            //    });
            //}


            optionList.AddElement(new GUITextBlock(Vector2.Zero, new Vector2(200, 10), "Environment",
                defaultStyle.TextFontStyle, Color.DarkSlateGray, Color.White, GUIStyle.TextAlignment.Center,
                Vector2.Zero));

            GuiListToggle environmentGroup = new GuiListToggle(Vector2.Zero, defaultStyle) { ToggleBlockColor = Color.DarkSlateGray, IsToggled = false };
            optionList.AddElement(environmentGroup);

            environmentGroup.AddElement(new GUITextBlockToggle(defaultStyle, "Enable Environment")
            {
                ToggleProperty = EnvironmentPipelineModule.ModuleEnabled.GetValuePropertyInfo(),
                ToggleObject = EnvironmentPipelineModule.ModuleEnabled,
                Toggle = EnvironmentPipelineModule.ModuleEnabled
            });

            _rightSideList.Alignment = GUIStyle.GUIAlignment.TopRight;
        }

        public void ChangeGizmoMode(GizmoModes mode)
        {
            _gizmoMode = mode;

            UpdateGizmoSelection(mode);
        }

        private void UpdateGizmoSelection(GizmoModes mode)
        {
            switch (mode)
            {
                case GizmoModes.Translation:
                    _gizmoTranslation.BlockColor = Color.MonoGameOrange;
                    _gizmoRotation.BlockColor = Color.Gray;
                    _gizmoScale.BlockColor = Color.Gray;
                    break;
                case GizmoModes.Rotation:
                    _gizmoTranslation.BlockColor = Color.Gray;
                    _gizmoRotation.BlockColor = Color.MonoGameOrange;
                    _gizmoScale.BlockColor = Color.Gray;
                    break;
                case GizmoModes.Scale:
                    _gizmoTranslation.BlockColor = Color.Gray;
                    _gizmoRotation.BlockColor = Color.Gray;
                    _gizmoScale.BlockColor = Color.MonoGameOrange;
                    break;
            }
        }

        private static string CreateHelperText()
        {
            return "Deferred Engine Controls\n" +
                    "Space - toggle on/off tools\n" +
                    "W A S D - move camera\n" +
                    "Right Mouse Button - Rotate Camera\n" +
                    "\n" +
                    "In Editor mode:\n" +
                    "Left Mouse Button - Select Object\n" +
                    "CTRL-C / Insert - duplicate object\n" +
                    "Del - delete object\n" +
                    "T - select translation gizmo\n" +
                    "R - select rotation gizmo\n" +
                    "Z - select scale gizmo\n" +
                   "\n" +
                   "F1 - Cycle through render targets\n";
        }

        public void Update(GameTime gameTime, bool isActive, TransformableObject selectedObject)
        {
            RenderingStats.UIIsHovered = false;
            if (!isActive || !RenderingSettings.e_IsEditorEnabled) return;

            if (_gizmoMode != _gizmoModePrevious)
            {
                _gizmoModePrevious = _gizmoMode;
                UpdateGizmoSelection(_gizmoModePrevious);
            }

            GUIControl.Update(Input.mouseLastState, Input.mouseState);

            if (GUIControl.GetMousePosition().X > _rightSideList.Position.X &&
                GUIControl.GetMousePosition().Y < _rightSideList.Dimensions.Y)
            {
                RenderingStats.UIIsHovered = true;
            }

            _leftSideList.IsHidden = !RenderingSettings.EnableSelection;

            if (selectedObject != null)
            {
                //Check if cached, otherwise apply

                if (activeObjectName != selectedObject.Name || activeObjectPos != selectedObject.Position)
                {
                    _objectDescriptionList.IsHidden = false;
                    _objectDescriptionName.Text.Clear();
                    _objectDescriptionName.Text.Append(selectedObject.Name);
                    _objectDescriptionName.TextAlignment = GUIStyle.TextAlignment.Center;

                    _objectDescriptionPos.Text.Clear();
                    _objectDescriptionPos.Text.AppendVector3(selectedObject.Position);
                    _objectDescriptionPos.TextAlignment = GUIStyle.TextAlignment.Center;

                    activeObjectName = selectedObject.Name;
                    activeObjectPos = selectedObject.Position;
                }

                _objectButton1.IsHidden = true;
                _objectToggle0.IsHidden = true;
                _objectToggle1.IsHidden = true;
                _objectToggle2.IsHidden = true;
                _objectToggle3.IsHidden = true;
                _objectSlider0.IsHidden = true;
                _objectSlider1.IsHidden = true;
                _objectSlider2.IsHidden = true;
                _objectColorPicker1.IsHidden = true;

                if (selectedObject is PointLight)
                {
                    _objectToggle0.IsHidden = false;
                    _objectToggle1.IsHidden = false;
                    _objectToggle2.IsHidden = false;
                    _objectToggle3.IsHidden = false;
                    _objectSlider0.IsHidden = false;
                    _objectSlider1.IsHidden = false;
                    _objectSlider2.IsHidden = false;
                    _objectColorPicker1.IsHidden = false;

                    if (activeObject != selectedObject)
                    {
                        _objectToggle0.SetProperty(selectedObject, "IsEnabled");
                        _objectToggle0.Text = new StringBuilder("IsEnabled");

                        _objectToggle1.SetField(selectedObject, "IsVolumetric");
                        _objectToggle1.Text = new StringBuilder("Volumetric");

                        _objectToggle2.SetField(selectedObject, "CastShadows");
                        _objectToggle2.Text = new StringBuilder("Cast Shadows");

                        _objectToggle3.SetField(selectedObject, "CastSDFShadows");
                        _objectToggle3.Text = new StringBuilder("Cast SDF Shadows");

                        _objectSlider0.MinValue = 1.1f;
                        _objectSlider0.MaxValue = 200;

                        _objectSlider0.SetProperty(selectedObject, "Radius");
                        _objectSlider0.SetText(new StringBuilder("Radius: "));

                        _objectSlider1.MinValue = 0.01f;
                        _objectSlider1.MaxValue = 300;

                        _objectSlider1.SetField(selectedObject, "Intensity");
                        _objectSlider1.SetText(new StringBuilder("Intensity: "));

                        _objectSlider2.SetValues("Shadow Softness: ", 1, 20, 1);
                        _objectSlider2.SetField(selectedObject, "ShadowMapRadius");

                        _objectColorPicker1.SetProperty(selectedObject, "Color");
                    }
                }

                else if (selectedObject is DirectionalLight)
                {
                    _objectToggle0.IsHidden = false;
                    _objectToggle2.IsHidden = false;
                    _objectSlider1.IsHidden = false;
                    _objectColorPicker1.IsHidden = false;

                    if (activeObject != selectedObject)
                    {
                        _objectToggle0.SetProperty(selectedObject, "IsEnabled");
                        _objectToggle0.Text = new StringBuilder("IsEnabled");

                        _objectToggle2.SetField(selectedObject, "CastShadows");
                        _objectToggle2.Text = new StringBuilder("Cast Shadows");

                        _objectSlider1.MinValue = 0.01f;
                        _objectSlider1.MaxValue = 300;

                        _objectSlider1.SetField(selectedObject, "Intensity");
                        _objectSlider1.SetText(new StringBuilder("Intensity: "));

                        _objectColorPicker1.SetProperty(selectedObject, "Color");
                    }
                }

                // Environment Sample!
                else if (selectedObject is EnvironmentProbe)
                {
                    _objectButton1.IsHidden = false;
                    _objectToggle1.IsHidden = false;
                    _objectToggle2.IsHidden = false;

                    _objectSlider0.IsHidden = false;
                    _objectSlider1.IsHidden = false;

                    if (activeObject != selectedObject)
                    {
                        _objectButton1.ButtonObject = selectedObject;
                        _objectButton1.ButtonMethod = selectedObject.GetType().GetMethod("Update");

                        _objectButton1.Text = new StringBuilder("Update Cubemap");

                        _objectToggle1.ToggleObject = selectedObject;
                        _objectToggle1.ToggleField = selectedObject.GetType().GetField("AutoUpdate");

                        _objectToggle1.Toggle = (selectedObject as EnvironmentProbe).AutoUpdate;

                        _objectToggle1.Text = new StringBuilder("Update on move");

                        _objectToggle2.SetField(selectedObject, nameof(EnvironmentProbe.UseSDFAO));
                        _objectToggle2.Text = new StringBuilder("Use SDFAO");

                        _objectSlider0.SetField(selectedObject, nameof(EnvironmentProbe.SpecularStrength));
                        _objectSlider0.SetValues("Specular Strength: ", 0.01f, 1, 2);

                        _objectSlider1.SetField(selectedObject, nameof(EnvironmentProbe.DiffuseStrength));
                        _objectSlider1.SetValues("Diffuse Strength: ", 0, 1, 2);
                    }
                }

                activeObject = selectedObject;
            }
            else
            {
                _objectDescriptionList.IsHidden = true;
            }

            GuiCanvas.Update(gameTime, GUIControl.GetMousePosition(), Vector2.Zero);
        }

        public void SetResolution(Vector2 resolution)
        {
            GUIControl.UpdateResolution(resolution);
            GuiCanvas.Resize(resolution);
        }
    }
}
