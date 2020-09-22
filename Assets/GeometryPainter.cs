using UIForia;
using UIForia.Graphics;
using UIForia.Rendering;
using UnityEngine;

namespace DefaultNamespace {

    // todo -- bring this back
    // [CustomPainter("GeometryPainter")]
    public class GeometryPainter : StandardRenderBox2 {

        private UIForiaMesh mesh;

        public override void Enable() {
            base.Enable();
        }

        public override void OnStylePropertyChanged(StyleProperty[] propertyList, int propertyCount) {
            base.OnStylePropertyChanged(propertyList, propertyCount);
            // if(propertyList[0].propertyId == )
        }

        public override void PaintBackground3(RenderContext3 ctx) {
            // 2 things
            // draw instances -> n:1 with element desc
            // draw thing -> 1:1 with element desc

            ctx.SetBackgroundColor(Color.red);
            for (int i = 0; i < 10; i++) {
                ctx.DrawCircle(i * 60, 0, 30);
                // ctx.SetOutlineColor(red);
                // ctx.SetBackgroundTexture(gradientxyz);
                // ctx.SetTextureUsage(alpha);
                // ctx.ComposeTexture();
                //
                // ctx.SetBackgroundTexture(texture, rect, uvTransform);
                // ctx.SetMatrix(float4x4.identity);
                // // ctx.DrawNineSlice(borderPaint, contentPaint, borderTop, borderRight, borderLeft, borderBottom);
                // ctx.SetMaskTexture(maskTexture, ...);
                // ctx.Reset();
                // ctx.SetBackgroundTexture(gradient, rect);
            }

            // mesh.Clear();
            // mesh.SetVertexColor();
            // mesh.SetAAWidth(1.25);
            // mesh.SetDpiScale(1.44);
            //
            // mesh.Clear();
            // ShapeKit.FillRect(ref vertexHelper, 10, 10, 20, 20);
            // mesh.FillRect();
            // mesh.Arc();
            //
            // vertexHelper.ToMesh(mesh);
            //
            // mesh.Add();
            //
            // ctx.DrawUIForiaMesh(mesh);
            //
            // ctx.SetBackgroundColor(red);
            //
            // ctx.DrawCircle(0, 0, 100);
            //
            // ctx.DrawDecoratedRect();
        }

        // gradients are images, punkt.
        // gradients are cpu generated?
        // how big are they? depends on element?
        // can be huge then..
        // if 1d then i can't do texture scroll in the same way
        // if 2d then i need to gen a lot more data
        // multiple bgs -> no batching, own texture
        // multiple gradients -> size = largest 

        // gradient2d {
        //  [size] 300px 300px; 
        //  [linear] 0px 100px 300px; 3000px;
        // }

        // style p {
        //     BackgroundImage = combine(add, gradiant0, blur(radius, texture));
        // }

        // texture-creator {
        //     add(texture, offset);
        //     subtract(gradient, mask);
        //     blur(5px);
        //     linear-gradient(50deg, red, blue);
        // }

        // generic draw api
        // draw some mesh with some material + batch it
        // blur + other effects
        // sdf paint api
        // render texture effect api
        // need to offset clip rect when rendering to non screen render target!!!
        // gradient texture generation

        // style painters probably only work with immediate mode sdf based drawing

        // other painters can listen to property changes etc
        // need to implement custom style properties of some sort

        // style.SetCustomProperty("name", 4);
        // style.GetCustomProperty<type>("name", default);
        // style { painter::Geometry.value = x }

        // <Element painter:geometry.value="xxx"/>
        // <Element material:name.value="xxx"/>

        // if element.painter == geometry
        // painter.value = xx;

    }

}