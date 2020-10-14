using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace SeedLib {
    
    public abstract class __ItemList : UIElement {
    
    } 

    [Template("SeedLib/List/ItemList.xml")]
    public class ItemList<T> : __ItemList {
        public string label;
        public IList<T> items;
        public Func<T, RepeatItemKey> keyFn;
    }

    [CustomPainter("SeedLib::ItemList")]
    public class ItemListPainter : StandardRenderBox {

        private Path2D path2D = new Path2D();

        public override void PaintBackground(RenderContext ctx) {
            base.PaintBackground(ctx);
            if (!element.TryGetAttribute("variant", out string attribute) || !attribute.Contains("striped")) {
                return;
            }
            
            UIElement repeatElement = element.FindFirstByType<UIRepeatElement>();
            UIElement parentElement = element.parent;
            float paddingLeft = parentElement.layoutResult.padding.left;
            float paddingRight = parentElement.layoutResult.padding.right;
                
            path2D.Clear();
            path2D.SetTransform(Matrix4x4.identity);
            path2D.SetFill(SeedLib.ThemeColors.coal50);
            
            float gap = repeatElement.style.GridLayoutRowGap * 0.5f;
            
            for (int i = 0; i < repeatElement.children.size; i++) {
                bool isEven = (i & 1) == 0;
                if (isEven) {
                    continue;
                }

                UIElement child = repeatElement.children[i][0];

                if (!(child is ListItem) || repeatElement.children[i].ChildCount != 1) {
                    continue;
                }
                
                path2D.BeginPath();
                Rect rect = child.layoutResult.ScreenRect;
                path2D.Rect(rect.x - paddingLeft, 
                        rect.y - gap,
                        rect.width + (paddingLeft + paddingRight), 
                        rect.height + (2*gap)
                );
                path2D.Fill();
            }
            ctx.DrawPath(path2D);
                      
        }
            
    }
    
}