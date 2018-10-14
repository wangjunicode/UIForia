using Rendering;
using UnityEditor;
using UnityEngine;

namespace Src.Editor {

    [CustomEditor(typeof(StyleDebugView))]
    public class StyleDebugViewEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            GUI.enabled = false;
            UIElement element = ((StyleDebugView) target).element;

            if (element.style?.computedStyle == null) {
                return;
            }
            ComputedStyle style = element.style.computedStyle;
            Color color = EditorGUILayout.ColorField("Background Color", style.BackgroundColor);

//            style.BackgroundColor = color;

//            if (element.parent.style != null && element.pstyle.LayoutType == LayoutType.Flex) {
//                EditorGUILayout.IntField("Flex Grow", style.FlexItemGrowthFactor);
//                EditorGUILayout.IntField("Flex Shrink", style.FlexItemShrinkFactor);
//                EditorGUILayout.EnumPopup("Self Alignment", style.FlexItemSelfAlignment);
//                Edt
//            }
            DrawMeasurement("Min Width", style.MinWidth);
            DrawMeasurement("Max Width", style.MaxWidth);
            DrawMeasurement("Preferred Width", style.PreferredWidth);

            DrawMeasurement("Min Height", style.MinHeight);
            DrawMeasurement("Max Height", style.MaxHeight);
            DrawMeasurement("Preferred Height", style.PreferredHeight);

            UIElement parent = element.Parent as UIElement;
            if (parent?.style.computedStyle.LayoutType == LayoutType.Grid) {
                EditorGUILayout.IntField("GridItem Col Start", IntUtil.IsDefined(style.GridItemColStart) ? style.GridItemColStart : -1);
                EditorGUILayout.IntField("GridItem Col Span", style.GridItemColSpan);
                EditorGUILayout.IntField("GridItem Row Start", IntUtil.IsDefined(style.GridItemRowStart) ? style.GridItemRowStart : -1);
                EditorGUILayout.IntField("GridItem Row Span", style.GridItemRowSpan);
            }

            EditorGUILayout.EnumPopup("Grid Item Col Self Align", style.GridItemColSelfAlignment);
            EditorGUILayout.EnumPopup("Grid Item Row Self Align", style.GridItemRowSelfAlignment);
            
            EditorGUILayout.Space();

            EditorGUILayout.RectField("LocalRect", element.layoutResult.LocalRect);
            EditorGUILayout.RectField("ScreenRect", element.layoutResult.ScreenRect);
            EditorGUILayout.RectField("ScreenOverflowRect", element.layoutResult.ScreenOverflowRect);
            EditorGUILayout.Vector2Field("Actual Size", element.layoutResult.actualSize);
            EditorGUILayout.Vector2Field("Allocated Size", element.layoutResult.allocatedSize);
            EditorGUILayout.Vector2Field("Content Size", element.layoutResult.contentSize);
            EditorGUILayout.Vector2Field("Content Offset", element.layoutResult.contentOffset);

            EditorGUILayout.Space();

            EditorGUILayout.EnumPopup("Layout Type", style.LayoutType);
            EditorGUILayout.EnumPopup("Layout Direction", style.FlexLayoutDirection);
            GUI.enabled = true;
        }

        public static void DrawMeasurement(string name, UIMeasurement measurement) {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(name);
            EditorGUILayout.FloatField(measurement.value);
            EditorGUILayout.EnumPopup(measurement.unit);

            EditorGUILayout.EndHorizontal();
        }

    }

}