using Rendering;
using UnityEditor;
using UnityEngine;

namespace Src.Editor {

    [CustomEditor(typeof(StyleDebugView))]
    public class StyleDebugViewEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            GUI.enabled = false;
            UIElement element = ((StyleDebugView) target).element;

            ComputedStyle style = element.style.computedStyle;

            Color color = EditorGUILayout.ColorField("Background Color", style.BackgroundColor);

//            style.BackgroundColor = color;

//            if (element.parent.style != null && element.pstyle.LayoutType == LayoutType.Flex) {
//                EditorGUILayout.IntField("Flex Grow", style.FlexItemGrowthFactor);
//                EditorGUILayout.IntField("Flex Shrink", style.FlexItemShrinkFactor);
//                EditorGUILayout.EnumPopup("Self Alignment", style.FlexItemSelfAlignment);
//                Edt
//            }
            // DrawMeasurement("Position X", element.style.positionX);
            // DrawMeasurement("Position Y", element.style.positionY);

            EditorGUILayout.Space();

            EditorGUILayout.RectField("LocalRect", element.layoutResult.LocalRect);
            EditorGUILayout.RectField("ScreenRect", element.layoutResult.ScreenRect);

            EditorGUILayout.Space();

//            EditorGUILayout.EnumPopup("Layout Type", element.style.layoutType);
//            EditorGUILayout.EnumPopup("Layout Direction", element.style.layoutDirection);
//            EditorGUILayout.EnumPopup("Layout Flow", element.style.layoutFlow);
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