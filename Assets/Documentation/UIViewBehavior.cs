using System;
using System.IO;
using CompilerTest;
using UIForia.Elements;
using UIForia.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia {

    public class UIViewBehavior : MonoBehaviour {

        public Type type;
        public string typeName;
        public new Camera camera;
        public Application application;
        public bool usePreCompiledTemplates;

        [HideInInspector] public string applicationName = "Game App 2";

        public TemplateSettings GetTemplateSettings(Type type) {
            TemplateSettings settings = new TemplateSettings();
            settings.rootType = type;
            settings.applicationName = applicationName;
            settings.assemblyName = "Assembly-CSharp";
            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "UIForiaGenerated2");
            settings.codeFileExtension = "generated.cs";
            settings.preCompiledTemplatePath = "Assets/UIForia_Generated2/" + applicationName;
            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);

            return settings;
        }

        float saturate(float x) {
            return math.max(0, math.min(1, x));
        }

        public Color SampleGradient(float sampleValue, Color[] colors, float2[] alphas, int colorCount, int alphaCount) {
            Color color = colors[0];
            float alpha = alphas[0].x;

            for (int idx = 1; idx < colorCount; idx++) {

                float prevTimeKey = colors[idx - 1].a;
                float colorPos = saturate((sampleValue - prevTimeKey) / (colors[idx].a - prevTimeKey));
                color = Color.Lerp(color, colors[idx], colorPos); // lerp(colorPos, step(0.5, colorPos), fixedOrBlend));
            }

            //float alphaPos = saturate((sampleValue - alphas[idx - 1].y) / (alphas[idx].y - alphas[idx - 1].y)) * step(idx, alphaCount - 1);
            //alpha = lerp(alpha, alphas[idx].x, lerp(alphaPos, step(0.5, alphaPos), fixedOrBlend));

            return new Color(color.r, color.g, color.b, alpha);
        }

        public void Start() {

            application = UIForiaRuntime.CreateGameApplication("GameApp", typeof(CompilerTestRoot));
            
            return;
            
#if UNITY_EDITOR
            QualitySettings.vSyncCount = 0; // VSync must be disabled for target frame rate to work
            UnityEngine.Application.targetFrameRate = 60;

            // int width = 128;
            // Texture2D texture2D = new Texture2D(width, 1, TextureFormat.ARGB32, false, false);
            //
            // Color[] colors = {
            //     new Color(0.529f, 0.227f, 0.706f, 0.0f),
            //     new Color(0.992f, 0.114f, 0.114f, 0.5f),
            //     new Color(0, 1f, 0, 1f),
            // };
            //
            // float2[] alphas = {
            //     new float2(1, 0f),
            //     new float2(1, 0.5f),
            //     new float2(1, 1f),
            // };
            //
            // for (int i = 0; i < width; i++) {
            //     texture2D.SetPixel(i, 0, SampleGradient((i / (float) width), colors, alphas, 3, 3));
            // }
            //
            // texture2D.SetPixel(0, 0, SampleGradient(0, colors, alphas, 3, 3));
            //
            // texture2D.filterMode = FilterMode.Trilinear;
            // texture2D.anisoLevel = 1;
            // texture2D.wrapMode = TextureWrapMode.Repeat;
            // texture2D.Apply();
            // GameObject.Find("RawImage").GetComponent<UnityEngine.UI.RawImage>().texture = texture2D;

            unsafe {
                // TextMaterialInfo m0 = new TextMaterialInfo();
                // Debug.Log(MurmurHash3.Hash((byte*)&m0, sizeof(TextMaterialInfo)));
                // m0.opacity = 1;
                // Debug.Log(MurmurHash3.Hash((byte*)&m0, sizeof(TextMaterialInfo)));
                // m0.opacity = 0.5f;
                // Debug.Log(MurmurHash3.Hash((byte*)&m0, sizeof(TextMaterialInfo)));
                // m0.opacity = 0f;
                // Debug.Log(MurmurHash3.Hash((byte*)&m0, sizeof(TextMaterialInfo)));

                // might need a new map type that doesnt hash my hashcode
                // binary search might be ok too using a split key/value scheme
                // Debug.Log(sizeof(TextureUsage));
                // hash returns linked list node
                // if no nodes, add it
                // if 1 node, return it
                // if more nodes, walk using memcmp
                // if found use
                // if not found append node, write data into array at given index
                // nice thing is this works for elements as well as text since its jsut bytes
                // i can get a big win by laying out the two structures coherently
                // reasonably confident with this that a ushort is enough address space 

            }

            AssertSize.AssertSizes();
#endif
            type = Type.GetType(typeName);
            if (type == null) return;

            TemplateSettings settings = GetTemplateSettings(type);

            // todo -- these should be moved to modules when they become supported
            settings.RegisterTextEffect("bounce", new TextEffectSpawner<BounceTextEffect>());

            settings.RegisterTextEffect("rotate-center", new TextEffectSpawner<RotateTextEffect, RotateTextEffect.EffectParameters>(
                new RotateTextEffect.EffectParameters() {
                    angleSpeed = 180,
                    angleDiffBetweenChars = 10
                }
            ));

#if UNITY_EDITOR
            application = usePreCompiledTemplates
                ? GameApplication.CreateFromPrecompiledTemplates(settings, camera, DoDependencyInjection)
                : GameApplication.CreateFromRuntimeTemplates(settings, camera, DoDependencyInjection);
#else
            application = GameApplication.CreateFromPrecompiledTemplates(settings, camera, DoDependencyInjection);
#endif
            // var element = application.GetElement(new ElementId(2));
            // element.style.SetBackgroundImage(texture2D, StyleState.Normal);
        }

        private void DoDependencyInjection(UIElement element) {
            // DiContainer.Inject(element);
        }

        private CommandBuffer commandBuffer;

        CommandBuffer cmd;
        private RenderTexture renderTexture;
        private Mesh mesh;
        private Mesh coverMesh;

        void OnPostRender() {

            if (application != null && commandBuffer != null) {
                commandBuffer.Clear();
                commandBuffer.ClearRenderTarget(true, false, Color.black, 1f);
                application.Render(application.Width, application.Height, commandBuffer); //camera.pixelWidth * application.DPIScaleFactor, camera.pixelHeight * application.DPIScaleFactor, commandBuffer);
            }

        }

        private void Update() {

            if (application != null) {
                application.DPIScaleFactor = 1;
                if (commandBuffer == null) {
                    commandBuffer = new CommandBuffer();
                    commandBuffer.name = "UIForia";
                    camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
                }

                application.Update();

            }

        }

        public void OnApplicationQuit() {
            application?.Dispose();
        }

    }

}