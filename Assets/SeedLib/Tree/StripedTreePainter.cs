using System.Collections.Generic;
using UIForia;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace SeedLib {
    [CustomPainter("SeedLib::StripedTreePainter")]
    public class StripedTreePainter : StandardRenderBox {
        private readonly Path2D path2D = new Path2D();
        private readonly List<Rect> rects = new List<Rect>();

        private static void CollectRectsToRender(UIElement root, List<Rect> outRects) {
            int flattenIndex = 0;
            CollectRectsToRender(root, outRects, ref flattenIndex);
        }

        private static void CollectRectsToRender(UIElement root, List<Rect> outRects, ref int flattenIndex) {
            for (int i = 0; i < root.children.Count; ++i) {
                UIElement child = root.children[i];
                if (child.isDisabled) {
                    continue;
                }

                switch (child) {
                    case TreeFoldout treeFoldout: {
                        if (!treeFoldout.layoutResult.isCulled) {
                            bool isEven = (flattenIndex & 1) == 0;
                            if (isEven) {
                                Rect titleScreenRect = treeFoldout.titleRow.layoutResult.ScreenRect;
                                float marginTop = treeFoldout.layoutResult.margin.top;
                                Rect rect = new Rect(titleScreenRect.x, titleScreenRect.y - marginTop * 0.5f, titleScreenRect.width, titleScreenRect.height + marginTop);
                                outRects.Add(rect);
                            }
                        }

                        flattenIndex++;

                        if (treeFoldout.expanded) {
                            CollectRectsToRender(treeFoldout.foldoutChildren, outRects, ref flattenIndex);
                        }

                        break;
                    }

                    case TreeNode treeNode: {
                        if (!treeNode.layoutResult.isCulled) {
                            bool isEven = (flattenIndex & 1) == 0;
                            if (isEven) {
                                Rect titleScreenRect = treeNode.layoutResult.ScreenRect;
                                float marginTop = treeNode.layoutResult.margin.top;
                                Rect rect = new Rect(titleScreenRect.x, titleScreenRect.y - marginTop * 0.5f, titleScreenRect.width, titleScreenRect.height + marginTop);
                                outRects.Add(rect);
                            }
                        }

                        flattenIndex++;
                        break;
                    }

                    // repeat with transclude is culled.
                    case UIRepeatElement repeat:
                        CollectRectsToRender(repeat, outRects, ref flattenIndex);
                        break;

                    default:
                        if (child.layoutResult.isCulled) {
                            break;
                        }

                        CollectRectsToRender(child, outRects, ref flattenIndex);
                        break;
                }
            }
        }

        public override void PaintBackground(RenderContext ctx) {
            base.PaintBackground(ctx);

            rects.Clear();
            CollectRectsToRender(element, rects);

            path2D.Clear();
            path2D.SetTransform(Matrix4x4.identity);
            path2D.SetFill(SeedLib.ThemeColors.coal50);

            UIElement parentElement = element.parent;
            
            Rect parentRect = parentElement.layoutResult.ScreenRect;
            // parent can have transclude style. To traverse up until found a rect with non-zero width.
            while (parentRect.width == 0 && parentElement.parent != null) {
                parentRect = parentElement.parent.layoutResult.ScreenRect;
            }

            for (int i = 0; i < rects.Count; ++i) {
                Rect rect = rects[i];

                path2D.BeginPath();
                path2D.Rect(parentRect.x, rect.y, parentRect.width, rect.height);
                path2D.Fill();
            }

            ctx.DrawPath(path2D);
        }
    }
}