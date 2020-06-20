using System.Diagnostics;
using UIForia.Compilers.Style;
using UIForia.Graphics;
using UIForia.Systems;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia.Rendering {

    [DebuggerDisplay("{element.ToString()}")]
    public class StylePainterRenderBox : RenderBox {

        public StylePainterDefinition painterDefinition;

        public override void OnInitialize() {
            HasForeground = painterDefinition.paintForeground != null;
        }

        StylePainterContext stylePainterContext = new StylePainterContext();

        public override void PaintBackground2(RenderContext2 ctx) {

            if (painterDefinition.paintBackground != null) {
                stylePainterContext.ctx = ctx;
                stylePainterContext._element = element;
                stylePainterContext._variables = painterDefinition.definedVariables;
                painterDefinition.paintBackground(stylePainterContext);
            }
            
            // ctx.SetColor(element.style.BackgroundColor);
            // ctx.FillRect(new float2(0, 0), new float2(100, 100));

            // ctx.GetGeometryRange(shapeid);
            //
            // ctx.GetGeometryRange(shapeRangeStart, shapeRangeEnd, out geometryBuffer);
            
            // ctx.SetUvs();
            
            // ctx.Text(text, font, position, width);

            // ctx.SetMaterial(null);

            // ctx.SetAntiAliasingWidth(1.25f);
            //
            // ctx.SetMaterial("name" || material);
            // ctx.SetMaterialFloat("name", value);
            //
            // ctx.DrawLine(points, lineInfo);
            //
            // ctx.SetTransform(matrix);
            //
            // ctx.PushClipRect();
            //
            // int id = ctx.FillRect();
            // ctx.PushClipShape(id);
            //
            // ctx.SetDrawMode(Opaque | Transparent | Shadow);
            //
            // ctx.SetOpacity();
            // ctx.SetClipRect();
            //
            // ctx.SetClipUVChannel(VertexChannel.TextureCoord3);
            // ctx.SetClipRectChannel(VertexChannel.TextureCoord2);
            //
            // ctx.SetOpacityChannel(VertexChannel.TextureCoord3, VertexComponent.X);
            // ctx.SetDPIScaleChannel(VertexChannel.TextureCoord3, VertexComponent.Y);
            // ctx.SetAdditionalVertexChannels();
            //
            // ctx.SetMainTexture();
            // ctx.SetMaskTexture();
            // ctx.SetClipTexture();
            //
            // ctx.ApplyVertexModifier(modifier);
            //
            // if (shadow) {
            //     ctx.SetDrawMode(Shadow);
            //     ctx.FillRect();
            //     ctx.SetDrawMode(Normal);
            // }
            //
            // var sk = new ShapeKit();
            //
            // // batching is based on material
            // // text is pre-rendered to buffer 
            // // then sampled as normal texture that is set globally or just fits in dynamic atlas
            //
            //
            // // mesh created by hand
            // // set vertex colors
            // // ie generate right now, get list back
            // ctx.DrawMesh(mesh, material, matrix);

        }

        public override void PaintBackground(RenderContext ctx) {

            // if (painterDefinition.paintBackground != null) {
            //     ctx.shapeContext.element = element;
            // }

            // for each render box that isn't culled
            // get a shape list
            // each shape has a matrix, usually the same as the element's
            // need to know which shapes might be transparent
            // aa boundary is transparent, body might be if textured
            // stream of render commands
            // streams get sorted by depth / layer, transparency
            // state changes need to be taken into effect
            // kind of need a scene graph
            
            // mesh generation done in parallel
            // returns ranges
            // shape defs sorted by depth 
            
            // state changes added to both transparent and opaque trees where it makes sense
            
            // standard, text, image boxes get bursted, custom painters do not
            
            // need to support custom meshes
            // need to support shapekit
            // need to support default UIForia shapes
            // no reason not to support 3d but that might be view based
            // render clipping != input clipping?
            // handle non rendered boxes
            

            Debug.Log(element.style.propertyMap[painterDefinition.definedVariables[0].propertyId].AsFloat);
            
            // ctx.shapeContext.SetColor(Color.cyan);
            // ctx.shapeContext.FillRect(100, 100);

            // shape boundaries
            // material change
            // texture change
            // material property set
            // pipeline set 
            
            //ctx.DrawShapeContext();
            
            // render goals:
            // async mesh generation, burst where possible
            // clipping / batching done independently if possible
            
            // shape generator
            // needs an id 
            // need a vertex helper/buffer
            // need to tell if shapes are opaque or not
            // can maybe submit aa to transparent and body to opaque
            
            
        }

    }

}