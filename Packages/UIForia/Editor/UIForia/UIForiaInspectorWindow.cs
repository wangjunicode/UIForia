using System;
using System.Collections.Generic;
using Shapes2D;
using TMPro;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UIForia.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Editor {

    public class UIForiaInspectorWindow : EditorWindow {

        private UIElement selectedElement;
        private Vector2 scrollPosition;
        private UIView view;
        private bool drawDebugBox;
        private Color overlayColor;
        private Vector3 drawPos;
        private float overlayBorderSize;
        private Color overlayBorderColor;

        private Color contentColor = new Color32(140, 182, 193, 175);
        private Color borderColor = new Color32(253, 221, 155, 175);
        private Color marginColor = new Color32(249, 204, 157, 175);
        private Color paddingColor = new Color32(196, 208, 139, 175);
        private Color baseLineColor = Color.red;
        private Color descenderColor = Color.blue;
        private Color outlineColor = new Color32(196, 208, 139, 175);
        private Material lineMaterial;
        private Mesh baselineMesh;
        private bool showTextBaseline;
        private bool showTextDescender;

        private readonly Dictionary<UIStyle, bool> m_ExpandedMap = new Dictionary<UIStyle, bool>();
        private static readonly GUIContent s_Content = new GUIContent();
        private static readonly StylePropertyIdComparer s_StyleCompare = new StylePropertyIdComparer();

        private static readonly Dictionary<Type, ValueTuple<int[], GUIContent[]>> m_EnumValueMap =
            new Dictionary<Type, ValueTuple<int[], GUIContent[]>>();

        private Mesh mesh;
        private Material material;
        private int tab;

        public static readonly string[] s_TabNames = {
            "Layout Result",
            "Applied Styles",
            "Computed Style",
            "Settings"
        };

        public void Update() {
            if (UIForiaHierarchyWindow.UIView != null) {
                if (view == null) {
                    view = UIForiaHierarchyWindow.UIView;
                    view.Application.RenderSystem.DrawDebugOverlay += DrawDebugOverlay;
                }
            }
            else {
                if (view != null) {
                    view.Application.RenderSystem.DrawDebugOverlay -= DrawDebugOverlay;
                    view = null;
                }
            }

            if (UIForiaHierarchyWindow.SelectedElement != selectedElement) {
                selectedElement = UIForiaHierarchyWindow.SelectedElement != null
                    ? UIForiaHierarchyWindow.SelectedElement
                    : null;
                m_ExpandedMap.Clear();
                Repaint();
            }
        }

        private bool showAllComputedStyles;
        private bool showComputedSources;

        private SearchField searchField;
        private string searchString = string.Empty;
        private static readonly int s_SizeKey = Shader.PropertyToID("_Size");
        private static readonly int s_ContentColorKey = Shader.PropertyToID("_ContentColor");
        private static readonly int s_PaddingColorKey = Shader.PropertyToID("_PaddingColor");
        private static readonly int s_BorderColorKey = Shader.PropertyToID("_BorderColor");
        private static readonly int s_MarginColorKey = Shader.PropertyToID("_MarginColor");
        private static readonly int s_MarginRectKey = Shader.PropertyToID("_MarginRect");
        private static readonly int s_BorderRectKey = Shader.PropertyToID("_BorderRect");
        private static readonly int s_PaddingRectKey = Shader.PropertyToID("_PaddingRect");
        private static readonly int s_ContentRectKey = Shader.PropertyToID("_ContentRect");
        private static readonly int s_BaseLineKey = Shader.PropertyToID("_BaseLine");
        private static readonly int s_DescenderKey = Shader.PropertyToID("_Descender");
        private static readonly int s_BaseLineColorKey = Shader.PropertyToID("_BaseLineColor");
        private static readonly int s_DescenderColorKey = Shader.PropertyToID("_DescenderColor");

        private void OnEnable() {
            searchField = new SearchField();
            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.ContentColor"), out contentColor)) {
                contentColor = new Color32(140, 182, 193, 175);
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.PaddingColor"), out borderColor)) {
                borderColor = new Color32(253, 221, 155, 175);
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.BorderColor"), out paddingColor)) {
                paddingColor = new Color32(253, 221, 155, 175);
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.MarginColor"), out marginColor)) {
                marginColor = new Color32(253, 221, 155, 175);
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.BaseLineColor"), out baseLineColor)) {
                baseLineColor = Color.red;
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.DescenderColor"), out descenderColor)) {
                descenderColor = Color.blue;
            }

            showTextBaseline = EditorPrefs.GetBool("UIForia.Inspector.ShowTextBaseline", false);
            showTextDescender = EditorPrefs.GetBool("UIForia.Inspector.ShowTextDescender", false);
            drawDebugBox = EditorPrefs.GetBool("UIForia.Inspector.DrawDebugBox", true);
        }

        private void DrawComputedStyle() {
            // style name, style value, source

            UIStyleSet style = selectedElement.style;

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Hover", StyleState.Hover);
            DrawStyleStateButton("Focus", StyleState.Focused);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Active", StyleState.Active);
            DrawStyleStateButton("Inactive", StyleState.Inactive);
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            showAllComputedStyles = EditorGUILayout.Toggle("Show All", showAllComputedStyles);
            showComputedSources = EditorGUILayout.Toggle("Show Sources", showComputedSources);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            searchString = searchField.OnGUI(searchString);
            GUILayout.Space(4);

            List<ValueTuple<string, StyleProperty>> properties = ListPool<ValueTuple<string, StyleProperty>>.Get();

            string lowerSearch = searchString.ToLower();

            for (int i = 0; i < StyleUtil.StylePropertyIdList.Length; i++) {
                StylePropertyId propertyId = StyleUtil.StylePropertyIdList[i];
                if (showAllComputedStyles || style.IsDefined(propertyId)) {
                    if (!string.IsNullOrEmpty(searchString)) {
                        string propertyName = StyleUtil.GetPropertyName(propertyId).ToLower();
                        if (!propertyName.Contains(lowerSearch)) {
                            continue;
                        }
                    }

                    string source = selectedElement.style.GetPropertySource(propertyId);
                    // todo double check this is right
                    properties.Add(ValueTuple.Create(source, style.GetPropertyValue(propertyId)));
                }
            }

            if (properties.Count == 0) {
                return;
            }

            if (showComputedSources) {
                properties.Sort((a, b) => {
                    if (a.Item1 == b.Item1) return 0;

                    bool aInstance = a.Item1.Contains("Instance");
                    bool bInstance = b.Item1.Contains("Instance");

                    if (aInstance && bInstance) {
                        return string.Compare(a.Item1, b.Item1, StringComparison.Ordinal);
                    }

                    if (aInstance) return -1;
                    if (bInstance) return 1;

                    bool aDefault = a.Item1.Contains("Default");
                    bool bDefault = b.Item1.Contains("Default");

                    if (aDefault && bDefault) {
                        return string.Compare(a.Item1, b.Item1, StringComparison.Ordinal);
                    }

                    if (aDefault) return 1;
                    if (bDefault) return -1;

                    return string.Compare(a.Item1, b.Item1, StringComparison.Ordinal);
                });

                GUILayout.Space(10);
                string currentSource = properties[0].Item1;
                EditorGUILayout.LabelField(currentSource);
                int start = 0;
                for (int i = 0; i < properties.Count; i++) {
                    if (currentSource != properties[i].Item1) {
                        properties.Sort(start, i - start, s_StyleCompare);

                        for (int j = start; j < i; j++) {
                            DrawStyleProperty(properties[j].Item2, false);
                        }

                        start = i;
                        currentSource = properties[i].Item1;
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField(currentSource);
                    }
                }

                properties.Sort(start, properties.Count - start, s_StyleCompare);
                for (int j = start; j < properties.Count; j++) {
                    DrawStyleProperty(properties[j].Item2, false);
                }
            }
            else {
                properties.Sort(0, properties.Count - 1, s_StyleCompare);
                for (int i = 0; i < properties.Count; i++) {
                    DrawStyleProperty(properties[i].Item2, false);
                }
            }
        }

        private void DrawDebugOverlay(LightList<RenderData> renderData, LightList<RenderData> drawList, Vector3 origin, Camera camera) {
            if (!drawDebugBox) return;
            if (material == null) {
                material = new Material(Resources.Load<Material>("UIForia/Materials/UIForiaDebug"));
            }

            if (selectedElement != null) {
                RenderData data = drawList.Find((d) => d.element == selectedElement);
                if (data == null) {
                    return;
                }

                LayoutResult result = selectedElement.layoutResult;

                Vector3 renderPosition = data.renderPosition;
                renderPosition.z = 5;

                OffsetRect padding = view.Application.LayoutSystem.GetPaddingRect(selectedElement);
                OffsetRect margin = view.Application.LayoutSystem.GetMarginRect(selectedElement);
                OffsetRect border = view.Application.LayoutSystem.GetBorderRect(selectedElement);

                float width = result.actualSize.width;
                float height = result.actualSize.height;

                Size renderSize = new Size(
                    width + margin.left + margin.right,
                    height + margin.top + margin.bottom
                );

                material.SetVector(s_SizeKey, new Vector4(renderSize.width, renderSize.height, 0, 0));
                material.SetColor(s_ContentColorKey, contentColor);
                material.SetColor(s_PaddingColorKey, paddingColor);
                material.SetColor(s_BorderColorKey, borderColor);
                material.SetColor(s_MarginColorKey, marginColor);

                mesh = MeshUtil.ResizeStandardUIMesh(mesh, renderSize);

                material.SetVector(s_MarginRectKey, new Vector4(
                    0, 0, width + margin.right + margin.left, height + margin.top + margin.bottom
                ));

                material.SetVector(s_BorderRectKey, new Vector4(
                    margin.left,
                    margin.top,
                    renderSize.width - margin.Horizontal,
                    renderSize.height - margin.Vertical
                ));

                material.SetVector(s_PaddingRectKey, new Vector4(
                    margin.left + border.left,
                    margin.top + border.top,
                    renderSize.width - margin.Horizontal - border.Horizontal,
                    renderSize.height - margin.Vertical - border.Vertical
                ));

                material.SetVector(s_ContentRectKey, new Vector4(
                    margin.left + border.left + padding.left,
                    margin.top + border.top + padding.top,
                    renderSize.width - margin.Horizontal - border.Horizontal - padding.Horizontal,
                    renderSize.height - margin.Vertical - border.Vertical - padding.Vertical
                ));

                Graphics.DrawMesh(mesh, renderPosition + origin - new Vector3(margin.left, -margin.top),
                    Quaternion.identity, material, 0, camera, 0, null, false, false, false);

                if (selectedElement is UITextElement && (showTextBaseline || showTextDescender)) {
                    baselineMesh = MeshUtil.ResizeStandardUIMesh(baselineMesh, new Size(width, height + 100));
                    UIStyleSet style = selectedElement.style;
                    TMP_FontAsset asset = style.TextFontAsset;
                    float s = (style.TextFontSize / asset.fontInfo.PointSize) * asset.fontInfo.Scale;

                    lineMaterial = lineMaterial ? lineMaterial : Resources.Load<Material>("Materials/UIForiaTextDebug");
                    float offset = TextLayoutBox.GetLineOffset(style.TextFontAsset);
                    if (showTextBaseline) {
                        lineMaterial.SetFloat(s_BaseLineKey,
                            offset + padding.top + border.top +
                            (s * style.TextFontAsset.fontInfo.Ascender));
                    }
                    else {
                        lineMaterial.SetFloat(s_BaseLineKey, -1);
                    }

                    if (showTextDescender) {
                        lineMaterial.SetFloat(s_DescenderKey,
                            offset + padding.top + border.top +
                            (s * style.TextFontAsset.fontInfo.Ascender) +
                            (s * -style.TextFontAsset.fontInfo.Descender)
                        );
                    }
                    else {
                        lineMaterial.SetFloat(s_DescenderKey, -1);
                    }

                    lineMaterial.SetColor(s_BaseLineColorKey, baseLineColor);
                    lineMaterial.SetColor(s_DescenderColorKey, descenderColor);
                    lineMaterial.SetVector(s_SizeKey, new Vector4(width, height + 100, 0, 0));

                    Graphics.DrawMesh(baselineMesh, renderPosition + origin, Quaternion.identity, lineMaterial, 0,
                        camera, 0, null, false, false, false);
                }
            }
        }


        private void DrawSettings() {
            bool newShowBaseLine = EditorGUILayout.Toggle("Show Text Baseline", showTextBaseline);
            bool newShowDescenderLine = EditorGUILayout.Toggle("Show Text Descender", showTextDescender);

            drawDebugBox = EditorGUILayout.Toggle("Draw Debug Box", drawDebugBox);
            
            Color newContentColor = EditorGUILayout.ColorField("Content Color", contentColor);
            Color newPaddingColor = EditorGUILayout.ColorField("Padding Color", paddingColor);
            Color newBorderColor = EditorGUILayout.ColorField("Border Color", borderColor);
            Color newMarginColor = EditorGUILayout.ColorField("Margin Color", marginColor);

            Color newBaseLineColor = EditorGUILayout.ColorField("Text Baseline Color", baseLineColor);
            Color newDescenderColor = EditorGUILayout.ColorField("Text Descender Color", descenderColor);

            if (newContentColor != contentColor) {
                contentColor = newContentColor;
                EditorPrefs.SetString("UIForia.Inspector.ContentColor", "#" + ColorUtility.ToHtmlStringRGBA(contentColor));
            }

            if (newPaddingColor != paddingColor) {
                paddingColor = newPaddingColor;
                EditorPrefs.SetString("UIForia.Inspector.PaddingColor", "#" + ColorUtility.ToHtmlStringRGBA(paddingColor));
            }

            if (newBorderColor != borderColor) {
                borderColor = newBorderColor;
                EditorPrefs.SetString("UIForia.Inspector.BorderColor", "#" + ColorUtility.ToHtmlStringRGBA(borderColor));
            }

            if (marginColor != newMarginColor) {
                marginColor = newMarginColor;
                EditorPrefs.SetString("UIForia.Inspector.MarginColor", "#" + ColorUtility.ToHtmlStringRGBA(marginColor));
            }

            if (baseLineColor != newBaseLineColor) {
                baseLineColor = newBaseLineColor;
                EditorPrefs.SetString("UIForia.Inspector.BaseLineColor", "#" + ColorUtility.ToHtmlStringRGBA(baseLineColor));
            }

            if (descenderColor != newDescenderColor) {
                descenderColor = newDescenderColor;
                EditorPrefs.SetString("UIForia.Inspector.DescenderColor",
                    ColorUtility.ToHtmlStringRGBA(descenderColor));
            }

            if (newShowBaseLine != showTextBaseline) {
                showTextBaseline = newShowBaseLine;
                EditorPrefs.SetBool("UIForia.Inspector.ShowTextBaseline", showTextBaseline);
            }

            if (newShowDescenderLine != showTextDescender) {
                showTextDescender = newShowDescenderLine;
                EditorPrefs.SetBool("UIForia.Inspector.ShowTextDescender", showTextDescender);
            }
            
            EditorPrefs.SetBool("UIForia.Inspector.DrawDebugBox", drawDebugBox);
        }

        private static void DrawLabel(string label, string value) {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            EditorGUILayout.LabelField(value);
            GUILayout.EndHorizontal();
        }

        private static void DrawVector2Value(string label, Vector2 value) {
            DrawLabel(label, $"X: {value.x}, Y: {value.y}");
        }

        private static void DrawSizeValue(string label, Size value) {
            DrawLabel(label, $"Width: {value.width}, Height: {value.height}");
        }

        private void DrawLayoutResult() {
            GUI.enabled = true;
            LayoutResult layoutResult = selectedElement.layoutResult;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            Rect clipRect = layoutResult.clipRect;
            Rect contentRect = layoutResult.contentRect;

//            EditorGUILayout.HelpBox("Overflowing Horizontal", MessageType.Warning, true);
            DrawVector2Value("Local Position", layoutResult.localPosition);
            DrawVector2Value("Screen Position", layoutResult.screenPosition);
            DrawVector2Value("Scale", layoutResult.scale);
            DrawSizeValue("Allocated Size", layoutResult.allocatedSize);
            DrawSizeValue("Actual Size", layoutResult.actualSize);

            DrawLabel("Rotation", layoutResult.rotation.ToString());
            DrawLabel("Clip Rect", $"X: {clipRect.x}, Y: {clipRect.y}, W: {clipRect.width}, H: {clipRect.height}");
            DrawLabel("Content Rect",
                $"X: {contentRect.x}, Y: {contentRect.y}, W: {contentRect.width}, H: {contentRect.height}");

            DrawLabel("Render Layer", layoutResult.layer.ToString());
            DrawLabel("Z Index", layoutResult.zIndex.ToString());

            GUILayout.Space(16);

            OffsetRect margin = view.Application.LayoutSystem.GetMarginRect(selectedElement);
            DrawLabel("Margin Top", margin.top.ToString());
            DrawLabel("Margin Right", margin.right.ToString());
            DrawLabel("Margin Bottom", margin.bottom.ToString());
            DrawLabel("Margin Left", margin.left.ToString());

            GUILayout.Space(16);

            OffsetRect border = view.Application.LayoutSystem.GetBorderRect(selectedElement);

            DrawLabel("Border Top", border.top.ToString());
            DrawLabel("Border Right", border.right.ToString());
            DrawLabel("Border Bottom", border.bottom.ToString());
            DrawLabel("Border Left", border.left.ToString());

            GUILayout.Space(16);

            OffsetRect padding = view.Application.LayoutSystem.GetPaddingRect(selectedElement);
            DrawLabel("Padding Top", padding.top.ToString());
            DrawLabel("Padding Right", padding.right.ToString());
            DrawLabel("Padding Bottom", padding.bottom.ToString());
            DrawLabel("Padding Left", padding.left.ToString());

            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void DrawStyleStateButton(string name, StyleState styleState) {
            bool isInState = selectedElement.style.IsInState(styleState);
            s_Content.text = "Force " + name;
            bool toggle = EditorGUILayout.Toggle(s_Content, isInState);
            if (!isInState && toggle) {
                selectedElement.style.EnterState(styleState);
            }
            else if (isInState && !toggle) {
                selectedElement.style.ExitState(styleState);
            }
        }

        private void DrawStyles() {
            UIStyleSet styleSet = selectedElement.style;

            List<UIStyleGroup> baseStyles = styleSet.GetBaseStyles();

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Hover", StyleState.Hover);
            DrawStyleStateButton("Focus", StyleState.Focused);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Active", StyleState.Active);
            DrawStyleStateButton("Inactive", StyleState.Inactive);
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            EditorGUIUtility.labelWidth = labelWidth;

            EditorGUILayout.BeginVertical();

            UIStyleGroup instanceStyle = styleSet.GetInstanceStyle();
            if (instanceStyle != null) {
                baseStyles.Insert(0, instanceStyle);
            }

            for (int i = 0; i < baseStyles.Count; i++) {
                UIStyleGroup group = baseStyles[i];
                s_Content.text = group.name;

                if (group.normal != null) {
                    DrawStyle(group.name + " [Normal]", group.normal);
                }

                if (group.hover != null) {
                    DrawStyle(group.name + " [Hover]", group.hover);
                }

                if (group.focused != null) {
                    DrawStyle(group.name + " [Focus]", group.focused);
                }

                if (group.active != null) {
                    DrawStyle(group.name + " [Active]", group.active);
                }

                if (group.inactive != null) {
                    DrawStyle(group.name + " [Inactive]", group.inactive);
                }
            }

            ListPool<UIStyleGroup>.Release(ref baseStyles);
            GUILayout.EndVertical();
        }

        public void OnGUI() {
            EditorGUIUtility.wideMode = true;

            if (selectedElement == null) {
                GUILayout.Label("Select an element in the UIForia Hierarchy Window");
                return;
            }

            tab = GUILayout.Toolbar(tab, s_TabNames);

            EditorGUIUtility.labelWidth += 50;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            switch (tab) {
                case 0:
                    DrawLayoutResult();
                    break;
                case 1:
                    DrawStyles();
                    break;
                case 2:
                    DrawComputedStyle();
                    break;
                case 3:
                    DrawSettings();
                    break;
            }

            EditorGUIUtility.labelWidth -= 50;

            // set from code = defined & not in template & not in bound style 

            GUILayout.EndScrollView();
        }

        private void DrawStyle(string name, UIStyle style) {
            bool expanded = true;

            if (m_ExpandedMap.ContainsKey(style)) {
                m_ExpandedMap.TryGetValue(style, out expanded);
            }

            expanded = EditorGUILayout.Foldout(expanded, name);
            m_ExpandedMap[style] = expanded;

            if (expanded) {
                EditorGUI.indentLevel++;
                IReadOnlyList<StyleProperty> properties = style.Properties;
                // todo -- sort? 
                for (int i = 0; i < properties.Count; i++) {
                    DrawStyleProperty(properties[i], false);
                }

                EditorGUI.indentLevel--;
            }
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
//                case StylePropertyId.BackgroundGridSize:
//                case StylePropertyId.BackgroundLineSize:
                case StylePropertyId.BackgroundFillOffsetX:
                case StylePropertyId.BackgroundFillOffsetY:
                case StylePropertyId.BackgroundFillScaleX:
                case StylePropertyId.BackgroundFillScaleY:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.BackgroundFillRotation:
//                case StylePropertyId.BackgroundImageTileX:
//                case StylePropertyId.BackgroundImageTileY:
//                case StylePropertyId.BackgroundImageOffsetX:
//                case StylePropertyId.BackgroundImageOffsetY:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.BackgroundImage:
                case StylePropertyId.BackgroundImage1:
                case StylePropertyId.BackgroundImage2:
                case StylePropertyId.Cursor:
                    return DrawTextureAsset(property, isEditable);

                case StylePropertyId.BackgroundShapeType:
                case StylePropertyId.Opacity:
                    return DrawFloat(property, isEditable);

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
                    return DrawGridTemplate(property, isEditable);

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
                    return DrawFontAsset(property, isEditable);

                case StylePropertyId.TextFontSize:
                    return DrawInt(property, isEditable);

                case StylePropertyId.TextFontStyle:
                    // todo -- this needs to be an EnumFlags popup
                    return DrawEnum<Text.FontStyle>(property, isEditable);

                case StylePropertyId.TextAlignment:
                    return DrawEnum<Text.TextAlignment>(property, isEditable);

//                case StylePropertyId.TextWhitespaceMode:
//                    return DrawEnum<WhitespaceMode>(property, isEditable);
//
                case StylePropertyId.TextTransform:
                    return DrawEnum<TextTransform>(property, isEditable);

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

//                default:
                    //throw new ArgumentOutOfRangeException(property.propertyId.ToString());
            }

            return StyleProperty.Unset(property.propertyId);
        }

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
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            ValueTuple<int[], GUIContent[]> tuple = GetEnumValues<T>();

            int[] values = tuple.Item1;
            GUIContent[] displayOptions = tuple.Item2;
            int index = Array.IndexOf(values, property.valuePart0);
            int output = EditorGUILayout.IntPopup(s_Content, index, displayOptions, values);
            // unclear if output is a value or an index, I suspect index
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, output) : property;
        }

        private static StyleProperty DrawColor(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            Color value = EditorGUILayout.ColorField(s_Content, property.AsColor);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawInt(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            float value = EditorGUILayout.IntField(s_Content, property.AsInt);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawFloat(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            float value = EditorGUILayout.FloatField(s_Content, property.AsFloat);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawFixedLength(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUILayout.BeginHorizontal();
            GUI.enabled = isEditable;
            float value = EditorGUILayout.FloatField(s_Content, property.AsUIFixedLength.value);
            UIFixedUnit unit = (UIFixedUnit) EditorGUILayout.EnumPopup(property.AsUIFixedLength.unit);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, new UIFixedLength(value, unit)) : property;
        }

        private static StyleProperty DrawGridTrackSize(StyleProperty property, bool isEditable) {
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            float value = EditorGUILayout.FloatField(s_Content, property.AsGridTrackSize.minValue);
            GridTemplateUnit unit = (GridTemplateUnit) EditorGUILayout.EnumPopup(property.AsGridTrackSize.minUnit);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, new GridTrackSize(value, unit)) : property;
        }

        private static StyleProperty DrawMeasurement(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            float value = EditorGUILayout.FloatField(s_Content, property.AsUIMeasurement.value);
            UIMeasurementUnit unit = (UIMeasurementUnit) EditorGUILayout.EnumPopup(property.AsUIMeasurement.unit);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, new UIMeasurement(value, unit)) : property;
        }

        private static StyleProperty DrawTextureAsset(StyleProperty property, bool isEditable) {
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            Texture2D texture = property.AsTexture;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(StyleUtil.GetPropertyName(property));
            Texture2D newTexture = (Texture2D) EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, 0, 0, newTexture) : property;
        }

        private static StyleProperty DrawFontAsset(StyleProperty property, bool isEditable) {
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            TMP_FontAsset fontAsset = property.AsFont;

            TMP_FontAsset newFont = (TMP_FontAsset) EditorGUILayout.ObjectField(StyleUtil.GetPropertyName(property),
                fontAsset, typeof(TMP_FontAsset), false);

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, 0, 0, newFont) : property;
        }

        private static StyleProperty DrawGridTemplate(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            IReadOnlyList<GridTrackSize> template = property.AsGridTrackTemplate;
            if (template == null) {
                EditorGUILayout.LabelField("Undefined");
            }
            else {
                for (int i = 0; i < template.Count; i++) {
                    float value = EditorGUILayout.FloatField(s_Content, property.AsGridTrackSize.minValue);
                    GridTemplateUnit unit =
                        (GridTemplateUnit) EditorGUILayout.EnumPopup(property.AsGridTrackSize.minUnit);
                }
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, 0, 0, null) : property;
        }

    }

    public class StylePropertyIdComparer : IComparer<ValueTuple<string, StyleProperty>> {

        public int Compare(ValueTuple<string, StyleProperty> x, ValueTuple<string, StyleProperty> y) {
            return (int) x.Item2.propertyId > (int) y.Item2.propertyId ? 1 : -1;
        }

    }

}