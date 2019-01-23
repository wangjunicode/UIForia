using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVGX {

    public class SVGXWand {

        public static void Work() {
            SVGXWand wand = new SVGXWand();

            SVGXGroup other;
        }

    }

    public interface ISVGXRenderable { }

    public class SVGXPathElement : SVGXElement, ISVGXRenderable {

        private readonly PathCommand[] commands;

        public SVGXPathElement(string id, SVGXStyle style, SVGXTransform transform) : base(id, style, transform) { }

        public SVGXPathElement(PathCommand[] commands) : base(default, default, default) {
            this.commands = commands;
        }

        // todo track conditions for when this needs to be re-called 
        // todo add insert / remove section that won't re-flatten when not needed
        // etc
        public List<List<Vector2>> Flatten() {
            List<List<Vector2>> retn = new List<List<Vector2>>();
            List<Vector2> currentList = new List<Vector2>();
            float lastX = 0;
            float lastY = 0;
            for (int i = 0; i < commands.Length; i++) {
                PathCommand cmd = commands[i];


                switch (cmd.commandType) {
                    case SVGXPathCommandType.MoveToRelative:
                    case SVGXPathCommandType.MoveToAbsolute:
                        lastX = cmd.px;
                        lastY = cmd.py;
                        break;

                    case SVGXPathCommandType.LineToRelative:
                    case SVGXPathCommandType.LineToAbsolute:
                        lastX = cmd.px;
                        lastY = cmd.py;
                        currentList.Add(new Vector2(lastX, lastY));
                        break;

                    case SVGXPathCommandType.HorizontalLineToRelative:
                    case SVGXPathCommandType.HorizontalLineToAbsolute:
                        currentList.Add(new Vector2(cmd.py, lastY));
                        lastX = cmd.px;
                        lastY = cmd.py;
                        break;

                    case SVGXPathCommandType.VerticalLineToRelative:
                    case SVGXPathCommandType.VerticalLineToAbsolute:
                        currentList.Add(new Vector2(lastX, cmd.py));
                        lastX = cmd.px;
                        lastY = cmd.py;
                        break;

                    case SVGXPathCommandType.SmoothCurveToRelative:
                    case SVGXPathCommandType.SmoothCurveToAbsolute:
                    case SVGXPathCommandType.CubicCurveToRelative:
                    case SVGXPathCommandType.CubicCurveToAbsolute: {
                        Vector2 start = transform.TransformPoint(lastX, lastY);
                        Vector2 ctrl0 = transform.TransformPoint(cmd.c0x, cmd.c0y);
                        Vector2 ctrl1 = transform.TransformPoint(cmd.c1x, cmd.c1y);
                        Vector2 end = transform.TransformPoint(cmd.px, cmd.py);
                        currentList.AddRange(SVGXBezier.Tessellate(start, ctrl0, ctrl1, end));
                        lastX = cmd.px;
                        lastY = cmd.py;
                        break;
                    }

                    case SVGXPathCommandType.ArcToAbsolute:
                    case SVGXPathCommandType.ArcToRelative:

                        break;

                    case SVGXPathCommandType.Close:
                        if (currentList.Count > 0) {
                            retn.Add(currentList);
                            currentList = new List<Vector2>();
                        }

                        break;

                    case SVGXPathCommandType.QuadraticCurveToAbsolute:
                    case SVGXPathCommandType.QuadraticCurveToRelative: {
                        Vector2 start = new Vector2(lastX, lastY);
                        Vector2 ctrl = new Vector2(cmd.c0x, cmd.c0y);
                        Vector2 end = new Vector2(cmd.px, cmd.py);
                        currentList.AddRange(SVGXBezier.QuadraticCurve(start, ctrl, end));
                        lastX = cmd.px;
                        lastY = cmd.py;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (currentList.Count > 0) {
                retn.Add(currentList);
            }

            return retn;
        }

    }

}