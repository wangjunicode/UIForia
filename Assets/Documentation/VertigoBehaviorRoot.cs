using SVGX;
using UIForia.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Vertigo;

public class VertigoBehaviorRoot : MonoBehaviour {

    public new Camera camera;
    public VertigoContext ctx;

    public Texture2D bgImage;
    public Texture2D lightFrame;
    public float softness;
    public SpriteAtlas atlas;

    public float width = 128;
    public float height = 128;
    public Vector4 radii;
    public Color color;
    public Vector2 tiling = Vector2.one;
    public Vector2 offset = Vector2.zero;
    public Vector2 pivot = Vector2.zero;
    [Range(-360f, 360f)] public float rotation;
    public Rect uvRect = new Rect(0, 0, 1, 1);
    private CommandBuffer commandBuffer;

    private TextInfo textInfo;

    public void Start() {
        ctx = new VertigoContext();

        ctx.SetFillMaterial(ctx.materialPool.GetShared("Materials/VertigoDefault"));
        ctx.SetStrokeMaterial(ctx.materialPool.GetShared("Materials/VertigoDefault"));
        textInfo = new TextInfo("H", new SVGXTextStyle() {
            fontSize = 36
        });

        //textInfo.Layout();

        commandBuffer = new CommandBuffer();
        camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    }
    
    public void Update() {
        camera.orthographicSize = Screen.height * 0.5f;

    }

}