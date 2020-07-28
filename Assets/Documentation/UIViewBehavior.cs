using System;
using System.IO;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia {
    struct TextMaterialInfo__Sizetest {
    
        uint faceColor;
        uint outlineColor;
        uint glowColor;
        uint underlayColor;
        // float opacity;
        // float fontSize;
        uint outlineWSFaceDilate;
        //float outlineWidth;
        //float outlineSoftness;
        //float faceDilate;

        uint underlayDilateSoftness;
        byte glowOffset;
        byte glowPower;
        byte glowInner;
        byte glowOuter;
        float underlayX;
        float underlayY;

        public float2 uvScroll;
        public float2 outlineScroll;

    };
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

        public void Start() {

            unsafe {
                Debug.Log(sizeof(TextMaterialInfo__Sizetest));
                // could also be ranges of effect characters, does have to be a single one.
                
                // which works except for revealers
                // while a reveal is active, could just treat the whole text as though it were special
                
                // on the plus side im not mixing logic
                // when reveal completes the entire text is demoted to normal
                // when it begins or restarts, entire text is promoted to effect
                
                // still dont really know what that means do I?
                // reveal is intended to have a finite duration
                // effects are more persistent
                // what does that really mean?
                // that I scan each character for material / effect overrides?
                // other implications?
                // dont want separate draw infos for revealing
                // reveal can certainly be non linear
                // i just need to know if the reveal is active
                // does it have a lasting effect? maybe if you dont call clear it would?
                // thats not really the intention though
                // mixing effect with reveal
                // is it too much? effect starts when reaveal finishes? 
                // no i think not, check reveal state every frame if you want to know about it
                // RevealState = BeginReveal | Revealing | EndReveal | Revealed | BeginHide | Hiding | EndHide | Hidden
                // per character opacity byte is probably totally fine
                // might end up solving a lot of these problems
                
                // data management is the real question here
                //     per character effects
                //        dont want to store more data than I need to
                //        dont want to clear state that the user wants to use
                //        when user is done with some data they need to know to call clear()?
                //        use some other flag from effect to tell reveal not to clear data? otherwise when reveal completes it will clear its data out
                //        where does reveal data live? material buffer? global? global makes a lot of sense but requires more copies and a flag possibly. could use negative idx
                //        since effect data is persistent im fine with that living locally, but it probably makes more sense if it doesnt
                
                // so we have normal text, revealing text, effected text
                // can have flags for that no problem 
                // can maybe also have symbol level effects for lerping and animating spaces and stuff that impacts layout, called before offset effects
                
                // where / how do I cull text?
                // rendering is the most precise, but I need to submit text elements for rendering that might be 'culled'? maybe not, just set a don't cull me flag later
                // or compute text bounds after effects run, should work too!
                // either way I do need to additionally do culling for render setup
                // do it at the painter level -> less work overall, edges cases caught later
                
                // textEffect.OnApplyCharacterEffect();
                // textEffect.OnApplySymbolEffect(); 
                
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

#if UNITY_EDITOR
            AssertSize.AssertSizes();
#endif
            type = Type.GetType(typeName);
            if (type == null) return;

            TemplateSettings settings = GetTemplateSettings(type);
            // cmd = new CommandBuffer();
            // cmd.name = "DrawTest";
            // renderTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            // mesh = new Mesh();
            // coverMesh = new Mesh();
#if UNITY_EDITOR
            application = usePreCompiledTemplates
                ? GameApplication.CreateFromPrecompiledTemplates(settings, camera, DoDependencyInjection)
                : GameApplication.CreateFromRuntimeTemplates(settings, camera, DoDependencyInjection);
#else
            application = GameApplication.CreateFromPrecompiledTemplates(settings, camera, DoDependencyInjection);
#endif

        }

        // optional!
        private void DoDependencyInjection(UIElement element) {
            // DiContainer.Inject(element);
        }

        private CommandBuffer commandBuffer;

        CommandBuffer cmd;
        private RenderTexture renderTexture;
        private Mesh mesh;
        private Mesh coverMesh;

        unsafe void OnPreRender() {

            if (application != null && commandBuffer != null) {
                commandBuffer.Clear();
                application.Render(camera.pixelWidth, camera.pixelHeight, commandBuffer);
            }

        }

        private void Update() {

     
            if (application != null) {
                if (commandBuffer == null) {
                    commandBuffer = new CommandBuffer();
                    commandBuffer.name = "UIForia";
                    camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
                }

                application.DPIScaleFactor = 1; // todo -- remove this
                application.Update();

            }

            // cmd.Clear();
            // cmd.SetRenderTarget(renderTexture);
            // cmd.ClearRenderTarget(true, true, Color.white);
            // float halfWidth = renderTexture.width * 0.5f;
            // float halfHeight = renderTexture.height * 0.5f;
            //
            // Matrix4x4 origin = Matrix4x4.TRS(new Vector3(-halfWidth, halfHeight, 0), Quaternion.identity, Vector3.one);
            // cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.Ortho(-halfWidth, halfWidth, -halfHeight, halfHeight, -100, 100));
            //
            // UIVertexHelper vertexHelper = UIVertexHelper.Create(Allocator.Temp);
            //
            // using (ShapeKit shapeKit = new ShapeKit(Allocator.Temp)) {
            //
            //     // Set BlendOp to min
            //     // Set Blend params to One One
            //     // Set ZWrite On
            //     // Set ZTest to NotEqual
            //     // set depth to # of parent masks
            //     // draw parent shapes top down
            //     // subtract 1 from depth each time
            //     // draw child shape at depth 0
            //     // draw a rect over the whole space
            //     int start = vertexHelper.currentVertCount;
            //     shapeKit.AddQuad(ref vertexHelper, 0, 0, 256, 256, Color.green);
            //     for (int i = start; i < vertexHelper.currentVertCount; i++) {
            //         vertexHelper.positions[i].z = -2f;
            //     }
            //
            //     start = vertexHelper.currentVertCount;
            //     shapeKit.AddQuad(ref vertexHelper, 32, 32, 256 - 48, 256 - 48, new Color32(0, 0, 0, 128));
            //
            //     for (int i = start; i < vertexHelper.currentVertCount; i++) {
            //         vertexHelper.positions[i].z = -1f;
            //     }
            //     shapeKit.SetDpiScale(1);
            //     shapeKit.SetAntiAliasWidth(0);//1.25f);
            //     shapeKit.AddCircle(ref vertexHelper, new Rect(128, 128, 256, 256), new EllipseProperties() {
            //         fitting = EllipseFitting.Ellipse,
            //     }, Color.red);
            //
            //
            //     //vertexHelper.Clear();
            //     shapeKit.AddRect(ref vertexHelper, 0, 0, 256, 256, new Color32(0, 0, 0, 0));
            //     vertexHelper.FillMesh(mesh);
            //
            // }
            //
            // vertexHelper.Dispose();
            // cmd.DrawMesh(mesh, origin, Resources.Load<Material>("UIForiaShape"));
            // // cmd.DrawMesh(coverMesh, origin, Resources.Load<Material>("UIForiaClipBlend"));
            // UnityEngine.Graphics.ExecuteCommandBuffer(cmd);

        }

        public void OnApplicationQuit() {
            application?.Dispose();
        }

    }

}