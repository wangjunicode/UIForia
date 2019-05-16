using UnityEngine;
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

    public void Start() {
        ctx = new VertigoContext();
        
        ctx.SetFillMaterial(ctx.materialPool.GetShared("Materials/VertigoDefault"));
        ctx.SetStrokeMaterial(ctx.materialPool.GetShared("Materials/VertigoDefault"));
        
        commandBuffer = new CommandBuffer();
        camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    }

    public void Update() {
        camera.orthographicSize = Screen.height * 0.5f;

        ctx.Clear();
        shapeGen.Clear();

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

        ctx.FillRhombus(-200, 100, width, height);//100, 50);
        
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
        ctx.Render();
        ctx.Flush(camera, commandBuffer);
    }

}