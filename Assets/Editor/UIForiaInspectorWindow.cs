using System;
using System.Collections.Generic;
using Rendering;
using Shapes2D;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.StyleBindings;
using Src.Systems;
using Src.Util;
using UnityEditor;
using UnityEngine;

namespace Src.Editor {

    public class UIForiaInspectorWindow : EditorWindow {

        private UIElement selectedElement;
        private Vector2 scrollPosition;
        private UIView view;
        private bool drawDebugBox;
        
        public void Update() {
            
            if (UIForiaHierarchyWindow.UIView != null) {
                if (view == null) {
                    view = UIForiaHierarchyWindow.UIView;
                    view.RenderSystem.DrawDebugOverlay += DrawDebugOverlay;
                }
            }
            else {
                if (view != null) {
                    view.RenderSystem.DrawDebugOverlay -= DrawDebugOverlay;
                    view = null;
                }
            }

            if (UIForiaHierarchyWindow.SelectedElement != selectedElement) {
                selectedElement = UIForiaHierarchyWindow.SelectedElement != null
                    ? UIForiaHierarchyWindow.SelectedElement
                    : null;
                Repaint();
            }
        }

        private Mesh mesh;
        private Material material;
        private void DrawDebugOverlay(LightList<RenderData> renderData, LightList<RenderData> drawList, Vector3 origin, Camera camera) {

            if (material == null) {
                material = new Material(Resources.Load<Material>("Materials/UIForia"));
            }

            if (selectedElement != null) {
                RenderData data = drawList.Find((d) => d.element == selectedElement);
                if (data == null) {
                    return;
                }
                LayoutResult result = selectedElement.layoutResult;
                mesh = MeshUtil.ResizeStandardUIMesh(mesh, result.actualSize);
                material.EnableKeyword(StandardRenderer.k_FillType_Color);
                material.EnableKeyword(StandardRenderer.k_UseBorder);
                material.SetVector(StandardRenderer.s_ClipRectKey, new Vector4(0, 0, 1, 1));
                material.SetVector(StandardRenderer.s_ColorKey, drawColor);
                material.SetVector(StandardRenderer.s_FillOffsetScaleKey, new Vector4(0, 0, 1, 1));
                material.SetVector(StandardRenderer.s_BorderSizeKey, new Vector4(3, 3, 3, 3));
                material.SetVector(StandardRenderer.s_BorderRadiusKey, new Vector4());
                material.SetVector(StandardRenderer.s_BorderColorKey, Color.blue);
                material.SetVector(StandardRenderer.s_SizeKey, new Vector4(result.actualSize.width, result.actualSize.height, 0, 0));
                Vector3 renderPosition = data.renderPosition;
                renderPosition.z = drawPos.z;
                Graphics.DrawMesh(mesh, renderPosition + origin , Quaternion.identity, material, 0, camera, 0, null, false, false, false);
            }
            
        }

        private Color drawColor;
        private Vector3 drawPos;
        private static readonly GUIRect s_GUIRect = new GUIRect();

        public void OnGUI() {
            if (selectedElement == null) {
                GUILayout.Label("Select an element in the UIForia Hierarchy Window");
                return;
            }

            s_GUIRect.SetRect(new Rect(0, 0, position.width - 20f, 20000f));

            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPosition, new Rect(0, 0, position.width - 20f, position.height * 10));
            LayoutResult layoutResult = selectedElement.layoutResult;

            EditorGUIUtility.wideMode = true;

            drawPos = EditorGUI.Vector3Field(s_GUIRect.GetFieldRect(), "draw", drawPos);
            drawColor = EditorGUI.ColorField(s_GUIRect.GetFieldRect(), "overlay", drawColor);
            
            GUI.enabled = false;
            
            GUI.Label(s_GUIRect.GetFieldRect(), "Layout Result");
            s_GUIRect.GetFieldRect();

            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Local Position", layoutResult.localPosition);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Screen Position", layoutResult.ScreenPosition);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Scale", layoutResult.scale);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Content Offset", layoutResult.contentOffset);

            EditorGUI.FloatField(s_GUIRect.GetFieldRect(), "Rotation", layoutResult.rotation);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Allocated Size", layoutResult.allocatedSize);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Actual Size", layoutResult.actualSize);

            s_GUIRect.GetFieldRect();
            EditorGUI.RectField(s_GUIRect.GetFieldRect(2), "Clip Rect", layoutResult.clipRect);
            EditorGUI.IntField(s_GUIRect.GetFieldRect(), "Render Layer", layoutResult.layer);
            EditorGUI.IntField(s_GUIRect.GetFieldRect(), "Z Index", layoutResult.zIndex);

            GUI.enabled = true;

            ComputedStyle style = selectedElement.ComputedStyle;
            StyleState current = selectedElement.style.CurrentState;

            UIStyleSet styleSet = selectedElement.style;

            List<UIStyleGroup> baseStyles = styleSet.GetBaseStyles();

            GUIStyle boxStyle = GUI.skin.box;
            GUIStyleState cachedBoxState = boxStyle.normal;
            Color oldColor = cachedBoxState.textColor;
            cachedBoxState.textColor = Color.white;

            bool isHovered = selectedElement.style.IsInState(StyleState.Hover);
            bool toggle = EditorGUI.Toggle(s_GUIRect.GetFieldRect(), s_Content, isHovered);
            if (!isHovered && toggle) {
                selectedElement.style.EnterState(StyleState.Hover);
            }
            else if (isHovered && !toggle) {
                selectedElement.style.ExitState(StyleState.Hover);
            }

//            List<StyleBinding> constantStyleBindings = selectedElement.templateRef.constantStyleBindings;

//            if (constantStyleBindings != null) {
//                
//            }

            for (int i = 0; i < baseStyles.Count; i++) {
                UIStyleGroup group = baseStyles[i];
                s_Content.text = group.name;

                int fieldCount = 0;
                if (group.normal != null) {
                    fieldCount += group.normal.Properties.Count + 2;
                }

                if (group.hover != null) {
                    fieldCount += group.hover.Properties.Count + 2;
                }

                if (group.focused != null) {
                    fieldCount += group.focused.Properties.Count + 2;
                }

                Rect boxRect = s_GUIRect.PeekFieldRect(fieldCount);
                GUI.Box(boxRect, s_Content);
                s_GUIRect.GetFieldRect();

                if (group.normal != null) {
                    DrawStyle("[Normal]", group.normal);
                }

                if (group.hover != null) {
                    DrawStyle("[Hover]", group.hover);
                }

                if (group.focused != null) {
                    DrawStyle("[Focus]", group.focused);
                }

                if (group.active != null) {
                    DrawStyle("[Active]", group.active);
                }

                if (group.inactive != null) {
                    DrawStyle("[Inactive]", group.inactive);
                }
            }

            // set from code = defined & not in template & not in bound style 
            boxStyle.normal.textColor = oldColor;

            GUI.EndScrollView();
        }

        private static void DrawStyle(string name, UIStyle style) {
            EditorGUI.Foldout(s_GUIRect.GetFieldRect(), true, name);
            s_GUIRect.Indent(14f);
            IReadOnlyList<StyleProperty> properties = style.Properties;
            // todo -- sort? 
            for (int i = 0; i < properties.Count; i++) {
                DrawStyleProperty(properties[i], false);
            }

            s_GUIRect.Indent(-14f);
        }

        private static StyleProperty DrawStyleProperty(StyleProperty property, bool isEditable) {
            switch (property.propertyId) {
                case StylePropertyId.OverflowX:
                case StylePropertyId.OverflowY:
                    return DrawEnum<Overflow>(property, isEditable);

                case StylePropertyId.BackgroundColor:
                case StylePropertyId.BackgroundColorSecondary:
                case StylePropertyId.BorderColor:
                    return DrawColor(property, isEditable);

                case StylePropertyId.BackgroundFillType:
                    return DrawEnum<FillType>(property, isEditable);

                case StylePropertyId.BackgroundGradientType:
                    return DrawEnum<GradientType>(property, isEditable);

                case StylePropertyId.BackgroundGradientAxis:
                    return DrawEnum<GradientAxis>(property, isEditable);

                case StylePropertyId.BackgroundGradientStart:
                case StylePropertyId.BackgroundGridSize:
                case StylePropertyId.BackgroundLineSize:
                case StylePropertyId.BackgroundFillOffsetX:
                case StylePropertyId.BackgroundFillOffsetY:
                case StylePropertyId.BackgroundFillScaleX:
                case StylePropertyId.BackgroundFillScaleY:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.BackgroundFillRotation:
                case StylePropertyId.BackgroundImageTileX:
                case StylePropertyId.BackgroundImageTileY:
                case StylePropertyId.BackgroundImageOffsetX:
                case StylePropertyId.BackgroundImageOffsetY:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.BackgroundImage:
                case StylePropertyId.BackgroundImage1:
                case StylePropertyId.BackgroundImage2:
                    break;

                case StylePropertyId.BackgroundShapeType:
                case StylePropertyId.Opacity:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.Cursor:
                    break;
                case StylePropertyId.GridItemColStart:
                case StylePropertyId.GridItemColSpan:
                case StylePropertyId.GridItemRowStart:
                case StylePropertyId.GridItemRowSpan:
                    return DrawInt(property, isEditable);

                case StylePropertyId.GridItemColSelfAlignment:
                case StylePropertyId.GridItemRowSelfAlignment:
                    return DrawEnum<CrossAxisAlignment>(property, isEditable);

                case StylePropertyId.GridLayoutDirection:
                    return DrawEnum<LayoutDirection>(property, isEditable);

                case StylePropertyId.GridLayoutDensity:
                    return DrawEnum<GridLayoutDensity>(property, isEditable);

                case StylePropertyId.GridLayoutColTemplate:
                case StylePropertyId.GridLayoutRowTemplate:
                    break;

                case StylePropertyId.GridLayoutColAutoSize:
                case StylePropertyId.GridLayoutRowAutoSize:
                    return DrawGridTrackSize(property, isEditable);

                case StylePropertyId.GridLayoutColGap:
                case StylePropertyId.GridLayoutRowGap:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.GridLayoutColAlignment:
                case StylePropertyId.GridLayoutRowAlignment:
                    return DrawEnum<CrossAxisAlignment>(property, isEditable);

                case StylePropertyId.FlexLayoutWrap:
                    return DrawEnum<WrapMode>(property, isEditable);

                case StylePropertyId.FlexLayoutDirection:
                    return DrawEnum<LayoutDirection>(property, isEditable);

                case StylePropertyId.FlexLayoutMainAxisAlignment:
                    return DrawEnum<MainAxisAlignment>(property, isEditable);

                case StylePropertyId.FlexLayoutCrossAxisAlignment:
                case StylePropertyId.FlexItemSelfAlignment:
                    return DrawEnum<CrossAxisAlignment>(property, isEditable);

                case StylePropertyId.FlexItemOrder:
                case StylePropertyId.FlexItemGrow:
                case StylePropertyId.FlexItemShrink:
                    return DrawInt(property, isEditable);

                case StylePropertyId.MarginTop:
                case StylePropertyId.MarginRight:
                case StylePropertyId.MarginBottom:
                case StylePropertyId.MarginLeft:
                    return DrawMeasurement(property, isEditable);

                case StylePropertyId.BorderTop:
                case StylePropertyId.BorderRight:
                case StylePropertyId.BorderBottom:
                case StylePropertyId.BorderLeft:
                case StylePropertyId.PaddingTop:
                case StylePropertyId.PaddingRight:
                case StylePropertyId.PaddingBottom:
                case StylePropertyId.PaddingLeft:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.BorderRadiusTopLeft:
                case StylePropertyId.BorderRadiusTopRight:
                case StylePropertyId.BorderRadiusBottomLeft:
                case StylePropertyId.BorderRadiusBottomRight:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.TransformPositionX:
                case StylePropertyId.TransformPositionY:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.TransformScaleX:
                case StylePropertyId.TransformScaleY:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.TransformPivotX:
                case StylePropertyId.TransformPivotY:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.TransformRotation:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.TransformBehaviorX:
                case StylePropertyId.TransformBehaviorY:
                    return DrawEnum<TransformBehavior>(property, isEditable);

                case StylePropertyId.__TextPropertyStart__:
                case StylePropertyId.__TextPropertyEnd__:
                    break;
                case StylePropertyId.TextColor:
                    return DrawColor(property, isEditable);

                case StylePropertyId.TextFontAsset:
                    break;
                case StylePropertyId.TextFontSize:
                    return DrawInt(property, isEditable);

                case StylePropertyId.TextFontStyle:
                    return DrawEnum<TextUtil.FontStyle>(property, isEditable);

                case StylePropertyId.TextAnchor:
                    return DrawEnum<TextUtil.TextAlignment>(property, isEditable);

                case StylePropertyId.TextWhitespaceMode:
                    return DrawEnum<WhitespaceMode>(property, isEditable);

                case StylePropertyId.TextWrapMode:
                    return DrawEnum<WrapMode>(property, isEditable);

                case StylePropertyId.TextHorizontalOverflow:
                case StylePropertyId.TextVerticalOverflow:
                    break;
                case StylePropertyId.TextIndentFirstLine:
                    break;
                case StylePropertyId.TextIndentNewLine:
                    break;
                case StylePropertyId.TextLayoutStyle:
                    break;
                case StylePropertyId.TextAutoSize:
                    break;
                case StylePropertyId.TextTransform:
                    return DrawEnum<TextUtil.TextTransform>(property, isEditable);

                case StylePropertyId.MinWidth:
                case StylePropertyId.MaxWidth:
                case StylePropertyId.PreferredWidth:
                case StylePropertyId.MinHeight:
                case StylePropertyId.MaxHeight:
                case StylePropertyId.PreferredHeight:
                    return DrawMeasurement(property, isEditable);

                case StylePropertyId.LayoutType:
                    return DrawEnum<LayoutType>(property, isEditable);

                case StylePropertyId.LayoutBehavior:
                    return DrawEnum<LayoutBehavior>(property, isEditable);

                case StylePropertyId.AnchorTop:
                case StylePropertyId.AnchorRight:
                case StylePropertyId.AnchorBottom:
                case StylePropertyId.AnchorLeft:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.AnchorTarget:
                    return DrawEnum<AnchorTarget>(property, isEditable);

                case StylePropertyId.ZIndex:
                case StylePropertyId.RenderLayerOffset:
                    return DrawInt(property, isEditable);

                case StylePropertyId.RenderLayer:
                    return DrawEnum<RenderLayer>(property, isEditable);

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return StyleProperty.Unset(property.propertyId);
        }

        private static readonly GUIContent s_Content = new GUIContent();

        private static Dictionary<Type, ValueTuple<int[], GUIContent[]>> m_EnumValueMap = new Dictionary<Type, ValueTuple<int[], GUIContent[]>>();

        private static ValueTuple<int[], GUIContent[]> GetEnumValues<T>() {
            ValueTuple<int[], GUIContent[]> retn;
            if (!m_EnumValueMap.TryGetValue(typeof(T), out retn)) {
                T[] vals = (T[]) Enum.GetValues(typeof(T));
                int[] intValues = new int[vals.Length];
                GUIContent[] contentValues = new GUIContent[vals.Length];

                for (int i = 0; i < vals.Length; i++) {
                    intValues[i] = (int) (object) vals[i];
                    contentValues[i] = new GUIContent(vals[i].ToString());
                }

                retn = ValueTuple.Create(intValues, contentValues);
                m_EnumValueMap[typeof(T)] = retn;
            }

            return retn;
        }

        private static StyleProperty DrawEnum<T>(StyleProperty property, bool isEditable) {
            Rect rect = s_GUIRect.GetFieldRect();
            s_Content.text = StyleUtil.GetPropertyName(property);
            rect.width = EditorGUIUtility.labelWidth + 100;
            GUI.enabled = isEditable;
            ValueTuple<int[], GUIContent[]> tuple = GetEnumValues<T>();

            int[] values = tuple.Item1;
            GUIContent[] displayOptions = tuple.Item2;
            int index = Array.IndexOf(values, property.valuePart0);
            int output = EditorGUI.IntPopup(rect, s_Content, index, displayOptions, values);

            return isEditable ? new StyleProperty(property.propertyId, output) : property;
        }

        private static StyleProperty DrawColor(StyleProperty property, bool isEditable) {
            Rect rect = s_GUIRect.GetFieldRect();
            s_Content.text = StyleUtil.GetPropertyName(property);
            rect.width = EditorGUIUtility.labelWidth + 100;
            GUI.enabled = isEditable;
            Color value = EditorGUI.ColorField(rect, s_Content, property.AsColor);
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawInt(StyleProperty property, bool isEditable) {
            Rect rect = s_GUIRect.GetFieldRect();
            s_Content.text = StyleUtil.GetPropertyName(property);
            rect.width = EditorGUIUtility.labelWidth + 100;
            GUI.enabled = isEditable;
            float value = EditorGUI.IntField(rect, s_Content, property.AsInt);
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawFloat(StyleProperty property, bool isEditable) {
            Rect rect = s_GUIRect.GetFieldRect();
            s_Content.text = StyleUtil.GetPropertyName(property);
            rect.width = EditorGUIUtility.labelWidth + 100;
            GUI.enabled = isEditable;
            float value = EditorGUI.FloatField(rect, s_Content, property.AsFloat);
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawFixedLength(StyleProperty property, bool isEditable) {
            Rect rect = s_GUIRect.GetFieldRect();
            s_Content.text = StyleUtil.GetPropertyName(property);
            rect.width = EditorGUIUtility.labelWidth + 100;
            GUI.enabled = isEditable;
            float value = EditorGUI.FloatField(rect, s_Content, property.AsFixedLength.value);
            rect.x += 100 + EditorGUIUtility.labelWidth;
            rect.width = 100;
            UIFixedUnit unit = (UIFixedUnit) EditorGUI.EnumPopup(rect, property.AsFixedLength.unit);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, new UIFixedLength(value, unit)) : property;
        }

        private static StyleProperty DrawGridTrackSize(StyleProperty property, bool isEditable) {
            Rect rect = s_GUIRect.GetFieldRect();
            s_Content.text = StyleUtil.GetPropertyName(property);
            rect.width = EditorGUIUtility.labelWidth + 100;
            GUI.enabled = isEditable;
            float value = EditorGUI.FloatField(rect, s_Content, property.AsGridTrackSize.minValue);
            rect.x += 100 + EditorGUIUtility.labelWidth;
            rect.width = 100;
            GridTemplateUnit unit = (GridTemplateUnit) EditorGUI.EnumPopup(rect, property.AsGridTrackSize.minUnit);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, new GridTrackSize(value, unit)) : property;
        }

        private static StyleProperty DrawMeasurement(StyleProperty property, bool isEditable) {
            Rect rect = s_GUIRect.GetFieldRect();
            s_Content.text = StyleUtil.GetPropertyName(property);
            rect.width = EditorGUIUtility.labelWidth + 100;
            GUI.enabled = isEditable;
            float value = EditorGUI.FloatField(rect, s_Content, property.AsMeasurement.value);
            rect.x += 100 + EditorGUIUtility.labelWidth;
            rect.width = 100;
            UIMeasurementUnit unit = (UIMeasurementUnit) EditorGUI.EnumPopup(rect, property.AsMeasurement.unit);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, new UIMeasurement(value, unit)) : property;
        }

    }

}