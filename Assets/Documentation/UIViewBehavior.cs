using System;
using System.IO;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Text;
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
        public struct Gradient2 {

            // data fits in 2 float4s
            public Color32 c0;
            public Color32 c1;
            public Color32 c2;
            public Color32 c3;
            public Color32 c4;
        
            public ushort a0;
            public ushort a1;
        
            public ushort a2;
            public ushort a3;
        
            public ushort a4;
        
        }

        public void Start() {

#if UNITY_EDITOR
//                QualitySettings.vSyncCount = 0; // VSync must be disabled for target frame rate to work
//                UnityEngine.Application.targetFrameRate = 60;

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

        }

        private void DoDependencyInjection(UIElement element) {
            // DiContainer.Inject(element);
        }

        private CommandBuffer commandBuffer;

        CommandBuffer cmd;
        private RenderTexture renderTexture;
        private Mesh mesh;
        private Mesh coverMesh;

        void OnPreRender() {

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