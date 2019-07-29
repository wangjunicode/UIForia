using UIForia;
using UIForia.Rendering;
using UnityEngine;

[CustomPainter("Dissolve")]
public class DissolveEffect : StandardRenderBox {

    public override Rect RenderBounds => new Rect(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);

    public Material material;
    private UIForiaGeometry effectGeometry;
    
    public DissolveEffect() {
        material = new Material(Shader.Find("UIForia/Dissolve"));
        effectGeometry = new UIForiaGeometry();
    }

    public override void OnInitialize() {
        material.SetTexture("_NoiseTex", Resources.Load<Texture>("UIDissolveNoise_Demo1"));
    }
    
    public override void PaintBackground(RenderContext ctx) {
        
        effectGeometry.Clear();
        effectGeometry.Quad(element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);
        
        // size should probably be min of cliprect and descendent screenspace extents
        // todo -- geometry should not be just a quad & transformed by xform matrix. basically a world space render bound
        
        // want to get a transparent render target
        // draw into it
        // mask things to shade
        // do not dissolve things that should not dissolve
        
        // draw to transparent
        // set result as texture
        // apply dissolve based on alpha clip, if alpha of given pixel is zero return 0
        // then blend result into destination
        
        ctx.PushPostEffect(material, element.layoutResult.screenPosition, element.layoutResult.actualSize);
        
        base.PaintBackground(ctx);
    }

    public override void PaintForeground(RenderContext ctx) {
        ctx.PopPostEffect();
    }

}