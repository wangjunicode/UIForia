using System;
using System.IO;
using JetBrains.Rider.Unity.Editor;
using SVGX;
using TreeEditor;
using UIForia.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Vertigo;

public class VertigoBehaviorRoot : MonoBehaviour {

    public new Camera camera;
    public VertigoContext ctx;
    ShapeGenerator shapeGen = new ShapeGenerator();
    GeometryGenerator geo = new GeometryGenerator();
    private GeometryCache cache;

    public Texture2D bgImage;
    public Texture2D lightFrame;
    public float softness;
    public SpriteAtlas atlas;

    public float width;
    public float height;
    public Vector4 radii;
    
    private CommandBuffer commandBuffer;

    private UIForiaRenderContext gfx;
    private TextInfo textInfo;
    public void Start() {
        ctx = new VertigoContext();
        
        ctx.SetFillMaterial(ctx.materialPool.GetShared("Materials/VertigoDefault"));
        ctx.SetStrokeMaterial(ctx.materialPool.GetShared("Materials/VertigoDefault"));
        textInfo = new TextInfo(new TextSpan("Hello + World", new SVGXTextStyle() {
            fontSize =  15
        }));
        
        commandBuffer = new CommandBuffer();
        camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
        
        Material material1 = new Material(Shader.Find("Vertigo/VertigoSDF"));
        Material material2 = new Material(Shader.Find("Vertigo/VertigoSDF"));
        material2.mainTexture = Texture2D.blackTexture;
        Profiler.BeginSample("MATERIALS");
        material1.CopyPropertiesFromMaterial(material2);
        Profiler.EndSample();
        gfx = new UIForiaRenderContext();
    }
    
    
    public void Update() {
        
        camera.orthographicSize = Screen.height * 0.5f;

        long start = GC.GetTotalMemory(false);
        gfx.Clear();
        
        gfx.SetFillColor(Color.red);
        gfx.FillRect(0, 0, 100, 100);
        gfx.FillRect(-300, 300, 100, 100);
        
        // do path stuff
        // bake to geometry data
        // gfx.SetState(state);
        
        gfx.Render();
        long end = GC.GetTotalMemory(false);
        gfx.Flush(camera, commandBuffer);
        Debug.Log(end - start);

//        ctx.SetShader(ShaderSlot.Text, new UIForiaShader("Vertigo/VertigoSDFText", "keywords"));
//        
//        ctx.SetShaderSlot(ShaderSlot.Text);
//        ctx.SetStencil();
//        ctx.SetMainTexture();
//        ctx.SetPass(MaterialSlot.Text, 0);
        
//        Path2D path = new Path2D();
//        int pathId = path.Begin();
//        path.Clear();
//        path.MoveTo();
//        path.LineTo();
//        path.Whatever();
//        ctx.FillPath(path, pathId);
        
        //ctx.SetRenderQueue(renderQueue.Transparent, PluginSettings, 1);
        
//        Shape2 shapeId = ctx.FillText(100, 100, textInfo);
        
//        ctx.SetVertexColors(shapeId, Color.red);
//        ctx.GetTexCoord1(shapeId, list);
//        ctx.SetTexCoord1(shapeId, new Vector4());
        
//        ctx.Draw(geo, geo.GetShapeIndex("name"));
        
//        VertigoMaterial.GetInstance(material);
//        VertigoMaterial.GetInstance("material/name");
//        VertigoMaterial.GetInstance("shader/name");
//        VertigoMaterial.GetInstance(shader);
//        
//        // GetInstance() will copy values on draw but can be re-used, copy is pooled, instance is pooled
//        // GetShared() will store a reference on draw and can be re-used
//        // Don't need a million instances of materials, easily re-usable
//
//        ctx.SetTexture(MaterialSlot.All, ShaderKey.Culling, null);
//        ctx.SetMaterialUsageType(Shared | Instance);
//        ctx.SetMaterial(MaterialType.Shared, myMaterial, keywords.....);
//        ctx.SetMaterialRenderQueue();
//        ctx.SetMaterialPass(0);
//        ctx.SetFloatProperty(MaterialSlot.Fill, key, one);
//        ctx.GetFloatProperty(MaterialSlot.Fill);
//        ctx.SetMaterial(material, new MaterialProperty(key, intValue));
//        ctx.EnableShaderKeyword(MaterialSlot.Fill, "KEYWORD_ONE");
//        ctx.SetPasses(0, 1, 2, 3);
//        ctx.SetMaterialPass(0);
        
        VertigoMaterial textMaterial = ctx.materialPool.GetShared("Materials/VertigoText");
        textMaterial.SetMainTexture(textInfo.spanList[0].textStyle.font.atlas);
        
        ctx.SetFillColor(Color.black);
        ctx.SetTextMaterial(textMaterial);
        ctx.FillText(0, 0, textInfo);
        
//        ctx.SetFillColor(Color.green);
//        ctx.FillRoundedRect(0, 0, width, height, radii.x, radii.y, radii.z, radii.w);
//
//        ctx.SetFillColor(Color.green);
//        ctx.FillRoundedRect(0, 200, width, height, radii.x, radii.y, radii.z, radii.w);
//
//        ctx.SetFillColor(Color.yellow);
//        ctx.FillCircle(-200, -200, 100);
//        ctx.SetFillColor(Color.red);
//
//        ctx.FillEllipse(-200, -300, 100, 50);

//        ctx.FillRhombus(-200, 100, width, height);//100, 50);
        
//        ctx.StrokeCircle()
        // PushRenderTexture();
        // ctx.SaveState();
        // ctx.SetColorMask(ColorMask.Alpha);
        // ctx.FillCircle();

        // ctx.RestoreState();
        // RenderTexture maskTexture = ctx.Render();
        // ctx.PopRenderTexture();
        // ctx.SetMask(maskTexture, 0.4f);
        // ctx.FillRect(...., material);
//        ctx.FillCircleSDF();
//        ctx.StrokeCircleSDF();
//        
//        ctx.SetDefaultGeometryMode(GeometryMode.SDF);
//        
//        ctx.Circle(0, 0, 50, GeometryMode.SDF);

//        ctx.BeginShapeRange();
//        shapeGen.Rect();
//        shapeGen.Rect();
//        shapeGen.Rect();
//        shapeGen.Rect();
//        shapeGen.Rect();
//        geo.Fill(shapeGen, shapeGen.currentRange)
//        geo.Stroke(shapeGen, shapeGen.currentRange)
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Rect(0, 0, 100, 100);
//        ctx.Circle(0, 100, 100);
//        ctx.BeginPath();
//        ctx.LineTo()
//        ctx.LineTo()
//        ctx.LineTo()
//        ctx.LineTo()
//        ctx.LineTo()
//        ctx.CurveTo();
//        ctx.EndPath();
//        
//        ctx.BeginStrokePath();
//        ctx.BeginFillPath();
//        ctx.FillPath(new [] {
//            Path.MoveTo(),
//            Path.LineTo()
//        });
//        int myRect = ctx.Rect(0, 0, 100, 100);
//        
//        ctx.Fill();
//        
//        ctx.SetDefaultMaterial(bg);
//        
//        int id = ctx.FillRect(0, 100, 500, 22, bgMat); // immediate
//        ctx.SetVertexColor(id);
//        
//        ctx.StrokeRect(0, 100, 500, 200, bgMat);
//        
//        ctx.DrawMesh();
//        ctx.DrawParticles();
//        ctx.DrawSprite();
//        ctx.DrawGeometry();
//        ctx.Draw(); // already created geometry
//        GeometryCache cache = null;
//        cache.SetVertexColors(3, Color.black);
//            ctx.BeginPath();
//        ctx.LineTo();
//        
//        ctx.Fill(bgMat);
//        
//        ctx.SetMask(lightFrame, softness);
//        ctx.Fill(bgMat);

    }

}