using System.Diagnostics;
using UIForia.Compilers.Style;
using UnityEngine;

namespace UIForia.Rendering {

    [DebuggerDisplay("{element.ToString()}")]
    public class StylePainterRenderBox : RenderBox {

        public StylePainterDefinition painterDefinition;

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
            
            // standard, text, image boxes get bursted, custom painters do not
            
            // for opaque
            // 

            UnityEngine.Debug.Log(element.style.propertyMap[painterDefinition.definedVariables[0].propertyId].AsFloat);
            
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