using UIForia.Util;

namespace SVGX {

    public enum SVGXRenderCommandType {

        MoveTo,
        LineTo,
        CurveTo,
        Stroke,
        Fill,
        SetStroke,
        SetFill

    }
    
    public struct SVGXRenderCommand {

        public SVGXRenderCommandType commandType;
        
    
    }
    
    public class SVGXTree {

        private readonly LightList<SVGXRenderCommand> commandList;

        public SVGXTree() {
            commandList = new LightList<SVGXRenderCommand>(32);
        }
        
        public void Render(ImmediateRenderContext ctx) {
            int count = commandList.Count;
            SVGXRenderCommand[] commands = commandList.Array;
            for (int i = 0; i < count; i++) {
                
            } 
                
        }

    }
    
    public class SVGXParser {

        public SVGXTree Parse(string value) {
            return null;
        }

    }

}