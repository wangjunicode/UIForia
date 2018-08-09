using System.Collections.Generic;
using Src.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Rendering {

    public enum Visibility {

        Visible,
        Hidden,
        Unset
    }

    public enum TextOverflow {

        Wrap,
        Truncate,
        Overflow,
        Unset       

    }
    
    public class Background {

        public Color color;
        public Texture2D texture;
        public Material material;
        public Rect uvRect;

    }

    public enum BorderStyle {

        Solid,
        Dashed,
        Dotted,
        Unset

    }

    public class MaterialDesc {

        public Color backgroundColor;
        public BorderStyle borderStyle;
        public float borderRadius;
        public Texture2D backgroundImage;
        public Material material;
        public Image.FillMethod fillMethod;
        public Vector2 backgroundTiling;
        public Shadow shadow;
        public float opacity;
        public Visibility visibility;

    }

    public struct LayoutParameters {

        public int growthFactor;
        public int shrinkFactor;
        public UnitValue minWidth;
        public UnitValue maxWidth;
        public UnitValue minHeight;
        public UnitValue maxHeight;
        public UnitValue basisWidth;
        public UnitValue basisHeight;

    }

    public class TextStyle {

        public Color? color;
        public Font font;
        public int fontSize = -1;
        public FontStyle? fontStyle;
        public TextAnchor? alignment;

    }

    public class LayoutDesc {

        public LayoutType layoutType;
        public CrossAxisAlignment crossAxisAlignment;
        public MainAxisAlignment mainAxisAlignment;
        public LayoutDirection direction;
        public LayoutWrap wrap;

    }

    public enum LayoutWrap {

        None,
        Wrap,
        Reverse,
        Unset
    }

    public enum LayoutDirection {

        Row,
        Column,
        Unset

    }

    public enum LayoutType {

        Flex,
        Grid,
        Radial,
        VStack,
        HStack,
        None,
        Unset

    }

    public class UIStyle {

        private Background _background;
        private List<UIElement> elements;
        public UILayout layout;
        public ContentBox contentBox;
        public LayoutParameters layoutParameters;

        public TextStyle textStyle = new TextStyle();

        public const int UnsetIntValue = int.MaxValue;
        public const float UnsetFloatValue = float.MaxValue;
        public static readonly Color UnsetColorValue = new Color(0, 0, 0, 0);
        
        public UIStyle() {
            _background = new Background();
            _background.color = Color.white;
            layout = new UILayout_Auto();
            contentBox = new ContentBox();
            // size defaults {
            //    padding: 0px
            //    border: 0px;
            //    margin: 0px;
            //    width FitContent(100%)
            //    height FitContent(100%)
            //    getActualContentWidth() -> Text: return text measurement
            //                            -> Image w/ asset -> return asset dimensions
            // }
        }

        public void ComputeSize() {
            /*
             * <container fit=content>
             *     <item fit=fill parent>
             *     <item fit=none>
             *
             *
             *     <Button>
             *         <Section>
             *             <Image>
             *             <Text>
             *             <Icon?>
             * 
             * fit:none -> if width set respect width
             *             otherwise just expand to min(children, maxWidth)
             * fit:content -> ignore width, compute non relative children, compute relative children, expand to min(maxWidth, children)
             *
             * fit:fill parent -> ignore width. wait till parent compute finishes, use that
             * fit:fill template -> needs to defer
             * fit: fill view -> 
             * fit content applies to non relative children
             * once non relative children are computed
             * relative children are figured out
             * then layout runs
             *
             * fill template
             *
             * missing link between renderable & style
             * style.GetContentWidth(element.renderable);
             * renderable.HasImplicitSize();
             *
             * layout -> top down
             *     list - fit content min(contentSize, maxWidth)
             *         item  - fit content
             *             thing - fill parent
             *             text - implicit sized
             *         item  - fill parent
             *             thing
             *                 text - implicit
             *             thing
             *             thing
             *     list
             *         item
             *             thing
             *
             *    x
             *     {a,b,c}
             *
             *   compute non relatives
             *   recurse
             *
             * 
             */
            // 1. fixed 
            // 2. percent relative to x
            // 3. default 
            // 4. fit children
            // 5. fit parent
        }

        /*

        // content relative with parent relative child is an error
        content -> parent = error
            parent -> content = good
        fixed -> content = good
        fixed -> parent = good

        fixed elements can flex (grow/shrink)

        content relative needs preferred size of all children
        fixed does not need any preferred sizes other than its own
        parent relative needs its parent's preferred size

        preferred size comes in 2 flavors: implicit and explicit. explicit is set via styles
            preferred size of fixed = whatever is set, implicit perferred size = 0, grow = 1
        preferred size of content relative = sum of preferred size of children
        preferred size of parent relative = parent preferred size * relation amount

            content relative
        fixed
            fixed -> in a layout could grow / shrink
            parent relative
            content relative
        fixed
            content relative
        fixed 
            fixed
            fixed

            content relative
        parent relative
*/
        public Background background {
            get { return _background; }
            set {
                // if(element.renderState == hover)
                // return if(hover.isBackgroundSet) else return _background
                // for each element 
                // element.view.MarkForRendering(elements);
            }
        }

        public bool RequiresRendering() {
            // has background style 
            // has background, border, (outline) material, shadow, or opacity
            return true;
        }

    }

    // GetBackground(element)

}