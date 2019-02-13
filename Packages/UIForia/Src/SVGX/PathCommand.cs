using UIForia.Rendering;

namespace SVGX {

    public struct PathCommand {

        public readonly float px;

        public readonly float py;
        public readonly float c0x;
        public readonly float c0y;
        public readonly float c1x;
        public readonly float c1y;

        internal readonly SVGXPathCommandType commandType;

        private PathCommand(SVGXPathCommandType commandType, float x, float y, float c0x, float c0y, float c1x, float c1y) {
            this.commandType = commandType;
            this.px = x;
            this.py = y;
            this.c0x = c0x;
            this.c0y = c0y;
            this.c1x = c1x;
            this.c1y = c1y;
        }

        public bool IsLargeArc => BitUtil.GetHighBits((int) c1y) > 0;
        public bool IsSweepArc => BitUtil.GetLowBits((int) c1y) > 0;
        public float ArcRotation => c1x;
        public float ArcEndX => c0x;
        public float ArcEndY => c0y;
        public float ArcRX => px;
        public float ArcRY => py;
        
        public static PathCommand MoveTo(float x, float y) {
            return new PathCommand(SVGXPathCommandType.MoveToAbsolute, x, y, 0, 0, 0, 0);
        }

        public static PathCommand LineTo(float x, float y) {
            return new PathCommand(SVGXPathCommandType.LineToAbsolute, x, y, 0, 0, 0, 0);
        }

        public static PathCommand CubicCurveTo(float c0x, float c0y, float c1x, float c1y, float endX, float endY) {
            return new PathCommand(SVGXPathCommandType.CubicCurveToAbsolute, endX, endY, c0x, c0y, c1x, c1y);
        }

        public static PathCommand ArcTo(float rx, float ry, float rotation, bool arc, bool sweep, float endX, float endY) {
            int flag = BitUtil.SetHighLowBits(arc ? 1 : 0, sweep ? 1 : 0);
            return new PathCommand(SVGXPathCommandType.ArcToAbsolute, rx, ry, endX, endY, rotation, flag);
        }

        public static PathCommand HorizontalLineTo(float x) {
            return new PathCommand(SVGXPathCommandType.HorizontalLineToAbsolute, x, 0, 0, 0, 0, 0);
        }

        public static PathCommand VerticalLineTo(float y) {
            return new PathCommand(SVGXPathCommandType.VerticalLineToAbsolute, 0, y, 0, 0, 0, 0);
        }

        public static PathCommand SmoothCurveTo(float c0x, float c0y, float c1x, float c1y, float endX, float endY) {
            return new PathCommand(SVGXPathCommandType.SmoothCurveToAbsolute, c0x, c0y, c1x, c1y, endX, endY);
        }

        public static PathCommand QuadraticCurveTo(float c0x, float c0y, float endX, float endY) {
            return new PathCommand(SVGXPathCommandType.QuadraticCurveToAbsolute, c0x, c0y, 0, 0, endX, endY);
        }

    }

}