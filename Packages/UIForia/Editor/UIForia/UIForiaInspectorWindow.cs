using System;
using System.Collections.Generic;
using SVGX;
using TMPro;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIForia.Editor {

    public class UIForiaInspectorWindow : EditorWindow {

        private UIElement selectedElement;
        private Vector2 scrollPosition;
        private bool drawDebugBox;
        private Color overlayColor;
        private Vector3 drawPos;
        private float overlayBorderSize;
        private Color overlayBorderColor;

        private Color contentColor = new Color32(140, 182, 193, 175);
        private Color allocatedContentColor = new Color32(90, 212, 193, 175);
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
        private Application app;

        public static readonly string[] s_TabNames = {
            "Element",
            "Applied Styles",
            "Computed Style",
            "Settings"
        };

        public void Update() {
            if (!EditorApplication.isPlaying) {
                return;
            }

            if (app != UIForiaHierarchyWindow.s_SelectedApplication) {
                if (app != null) {
                    app.RenderSystem.DrawDebugOverlay -= DrawDebugOverlay;
                }

                app = UIForiaHierarchyWindow.s_SelectedApplication;
                if (app != null) {
                    app.RenderSystem.DrawDebugOverlay += DrawDebugOverlay;
                }

                m_ExpandedMap.Clear();
            }

            Repaint();
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

            bool isSet = (selectedElement.flags & UIElementFlags.DebugLayout) != 0;
            if (GUILayout.Toggle(isSet, "Debug Layout")) {
                selectedElement.flags |= UIElementFlags.DebugLayout;
            }
            else {
                selectedElement.flags &= ~UIElementFlags.DebugLayout;
            }
            
            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Hover", StyleState.Hover);
            DrawStyleStateButton("Focus", StyleState.Focused);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Active", StyleState.Active);
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
                    StyleProperty property = selectedElement.style.GetComputedStyleProperty(propertyId);
                    if (!property.hasValue) {
                        property = DefaultStyleValues_Generated.GetPropertyValue(propertyId);
                    }

                    properties.Add(ValueTuple.Create(source, property));
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

        private void DrawDebugOverlay(ImmediateRenderContext ctx) {
            if (!drawDebugBox) return;
            ctx.DisableScissorRect();
            ctx.SetFillOpacity(1);
            ctx.SetStrokeOpacity(1);
            if (material == null) {
                material = new Material(Resources.Load<Material>("UIForia/Materials/UIForiaDebug"));
            }

            if (selectedElement != null) {
                // RenderData data = drawList.Find((d) => d.element == selectedElement);
                //if (data == null) {
                //    return;
                //}

                LayoutResult result = selectedElement.layoutResult;

                //Vector3 renderPosition = data.renderPosition;
                //renderPosition.z = 5;

                OffsetRect padding = app.LayoutSystem.GetPaddingRect(selectedElement);
                OffsetRect margin = app.LayoutSystem.GetMarginRect(selectedElement);
                OffsetRect border = app.LayoutSystem.GetBorderRect(selectedElement);

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
                     width + margin.right + margin.left, height + margin.top + margin.bottom
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

                float x = result.screenPosition.x;
                float y = result.screenPosition.y;

                ctx.DisableScissorRect();
                ctx.SetTransform(SVGXMatrix.identity);
                ctx.SetFill(contentColor);
                float contentX = (result.screenPosition.x) + border.left + padding.left;
                float contentY = (result.screenPosition.y) + border.top + padding.top;
                float contentWidth = result.actualSize.width - border.Horizontal - padding.Horizontal;
                float contentHeight = result.actualSize.height - border.Vertical - padding.Vertical;
                float allocatedWidth = result.allocatedSize.width - border.Horizontal - padding.Horizontal;
                float allocatedHeight = result.allocatedSize.height - border.Vertical - padding.Vertical;
                ctx.FillRect(contentX, contentY, contentWidth, contentHeight);
                
                ctx.SetFill(allocatedContentColor);
                ctx.FillRect(contentX, contentY, allocatedWidth, allocatedHeight);

                float paddingHorizontalWidth = width - padding.Horizontal - border.left;
                float paddingVerticalHeight = height - border.Vertical;

                ctx.SetFill(paddingColor);
                if (padding.top > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x + padding.left + border.left, y + border.top, paddingHorizontalWidth, padding.top);
                    ctx.Fill();
                }

                if (padding.right > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x + width - padding.right - border.right, y + border.top, padding.right, paddingVerticalHeight);
                    ctx.Fill();
                }

                if (padding.left > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x + border.left, y + border.top, padding.left, paddingVerticalHeight);
                    ctx.Fill();
                }

                if (padding.bottom > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x + border.left + padding.left, y - border.top + height - padding.bottom, paddingHorizontalWidth, padding.bottom);
                    ctx.Fill();
                }

                ctx.SetFill(borderColor);

                if (border.top > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x + border.left, y, width - border.Horizontal, border.top);
                    ctx.Fill();
                }

                if (border.right > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x + width - border.right, y, border.right, height);
                    ctx.Fill();
                }

                if (border.left > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x, y, border.left, height);
                    ctx.Fill();
                }

                if (border.bottom > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x + border.left, y + height - border.bottom, width - border.Horizontal, border.bottom);
                    ctx.Fill();
                }

                ctx.SetFill(marginColor);
                if (margin.left > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x - margin.left, y, margin.left, height);
                    ctx.Fill();
                }

                if (margin.right > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x + width, y, margin.right, height);
                    ctx.Fill();
                }

                if (margin.top > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x - margin.left, y - margin.top, width + margin.Horizontal, margin.top);
                    ctx.Fill();
                }

                if (margin.bottom > 0) {
                    ctx.BeginPath();
                    ctx.Rect(x - margin.left, y + height, width + margin.Horizontal, margin.bottom);
                    ctx.Fill();
                }
                
                if (selectedElement.style.LayoutType != LayoutType.Grid) {
                    return;
                }

                if (!(selectedElement.Application.LayoutSystem.GetBoxForElement(selectedElement) is GridLayoutBox layoutBox)) {
                    return;
                }

                Rect contentRect = selectedElement.layoutResult.ContentRect;

                ctx.SetTransform(SVGXMatrix.TRS(selectedElement.layoutResult.screenPosition + selectedElement.layoutResult.ContentRect.min, 0, Vector2.one));
                ctx.BeginPath();
                ctx.SetStrokeWidth(1);
                ctx.SetStroke(Color.black);

                StructList<GridTrack> rows = layoutBox.GetRowTracks();
                StructList<GridTrack> cols = layoutBox.GetColTracks();

                for (int i = 0; i < rows.Count; i++) {
                    ctx.MoveTo(0, rows[i].position);
                    ctx.LineTo(contentRect.width, rows[i].position);
                }

                for (int i = 0; i < cols.Count; i++) {
                    ctx.MoveTo(cols[i].position, 0);
                    ctx.LineTo(cols[i].position, contentRect.height);
                }

                ctx.Stroke();
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

        private void DrawAttributes(List<ElementAttribute> attributes) {
            DrawLabel("Attributes", "");
            EditorGUI.indentLevel++;
            for (int i = 0; i < attributes.Count; i++) {
                DrawLabel(attributes[i].name, attributes[i].value);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawElementInfo() {
            List<ElementAttribute> attributes = selectedElement.GetAttributes();
            if (attributes != null) {
                DrawAttributes(attributes);
            }

            GUI.enabled = true;
            LayoutResult layoutResult = selectedElement.layoutResult;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            Rect clipRect = layoutResult.clipRect;
            Rect contentRect = layoutResult.ContentRect;

            DrawLabel("View", selectedElement.View.name);
            DrawLabel("Viewport", $"X: {selectedElement.View.Viewport.x}, Y: {selectedElement.View.Viewport.y}, W: {selectedElement.View.Viewport.width}, H: {selectedElement.View.Viewport.height}");
            DrawVector2Value("Local Position", layoutResult.localPosition);
            DrawVector2Value("Screen Position", layoutResult.screenPosition);
            DrawVector2Value("Scale", layoutResult.scale);
            DrawSizeValue("Allocated Size", layoutResult.allocatedSize);
            DrawSizeValue("Actual Size", layoutResult.actualSize);

            DrawLabel("Rotation", layoutResult.rotation.ToString());
            DrawLabel("Clip Rect", $"X: {clipRect.x}, Y: {clipRect.y}, W: {clipRect.width}, H: {clipRect.height}");
            DrawLabel("Content Rect",
                $"X: {contentRect.x}, Y: {contentRect.y}, W: {contentRect.width}, H: {contentRect.height}");

            DrawLabel("Render Layer", selectedElement.style.RenderLayer.ToString());
            DrawLabel("Z Index", layoutResult.zIndex.ToString());

            GUILayout.Space(16);

            DrawEnumWithValue<LayoutType>(selectedElement.style.GetComputedStyleProperty(StylePropertyId.LayoutType), false);
            DrawMeasurement(selectedElement.style.GetComputedStyleProperty(StylePropertyId.PreferredWidth), false);
            DrawMeasurement(selectedElement.style.GetComputedStyleProperty(StylePropertyId.PreferredHeight), false);
            
            GUILayout.Space(16);
            
            OffsetRect margin = app.LayoutSystem.GetMarginRect(selectedElement);
            DrawLabel("Margin Top", margin.top.ToString());
            DrawLabel("Margin Right", margin.right.ToString());
            DrawLabel("Margin Bottom", margin.bottom.ToString());
            DrawLabel("Margin Left", margin.left.ToString());

            GUILayout.Space(16);

            OffsetRect border = selectedElement.layoutResult.border;

            DrawLabel("Border Top", border.top.ToString());
            DrawLabel("Border Right", border.right.ToString());
            DrawLabel("Border Bottom", border.bottom.ToString());
            DrawLabel("Border Left", border.left.ToString());

            GUILayout.Space(16);

            OffsetRect padding = selectedElement.layoutResult.padding;
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

            IList<UIStyleGroupContainer> baseStyles = styleSet.GetBaseStyles();

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Hover", StyleState.Hover);
            DrawStyleStateButton("Focus", StyleState.Focused);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Active", StyleState.Active);
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            EditorGUIUtility.labelWidth = labelWidth;

            EditorGUILayout.BeginVertical();
//
//            UIStyleGroup instanceStyle = styleSet.GetInstanceStyle();
//            if (instanceStyle != null) {
//                baseStyles.Insert(0, instanceStyle);
//            }
//
//            for (int i = 0; i < baseStyles.Count; i++) {
//                UIStyleGroupContainer group = baseStyles[i];
//                s_Content.text = $"{group.name} ({group.styleType.ToString()})";
//
//                if (group.normal != null) {
//                    DrawStyle(s_Content.text + " [Normal]", group.normal);
//                }
//
//                if (group.hover != null) {
//                    DrawStyle(s_Content.text + " [Hover]", group.hover);
//                }
//
//                if (group.focused != null) {
//                    DrawStyle(s_Content.text + " [Focus]", group.focused);
//                }
//
//                if (group.active != null) {
//                    DrawStyle(s_Content.text + " [Active]", group.active);
//                }
//            }
//
//            ListPool<UIStyleGroup>.Release(ref baseStyles);
            GUILayout.EndVertical();
        }

        public void OnGUI() {
            EditorGUIUtility.wideMode = true;

            if (app == null) {
                return;
            }

            int elementId = UIForiaHierarchyWindow.s_SelectedElementId;

            selectedElement = app.GetElement(elementId);

            if (selectedElement == null) {
                GUILayout.Label("Select an element in the UIForia Hierarchy Window");
                return;
            }

            tab = GUILayout.Toolbar(tab, s_TabNames);

            EditorGUIUtility.labelWidth += 50;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            switch (tab) {
                case 0:
                    DrawElementInfo();
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
                // todo -- sort? 
                for (int i = 0; i < style.PropertyCount; i++) {
                    DrawStyleProperty(style[i], false);
                }

                EditorGUI.indentLevel--;
            }
        }

        private static StyleProperty DrawStyleProperty(StyleProperty property, bool isEditable) {
            switch (property.propertyId) {
                case StylePropertyId.OverflowX:
                case StylePropertyId.OverflowY:
                    return DrawEnumWithValue<Overflow>(property, isEditable);

                case StylePropertyId.BackgroundColor:
                case StylePropertyId.BorderColor:
                    return DrawColor(property, isEditable);

                case StylePropertyId.Visibility:
                    return DrawEnumWithValue<Visibility>(property, isEditable);

                case StylePropertyId.Painter:
                    return DrawString(property, isEditable);

//                case StylePropertyId.BackgroundGridSize:
//                case StylePropertyId.BackgroundLineSize:
                case StylePropertyId.BackgroundImageOffsetX:
                case StylePropertyId.BackgroundImageOffsetY:
                case StylePropertyId.BackgroundImageScaleX:
                case StylePropertyId.BackgroundImageScaleY:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.BackgroundImageRotation:
//                case StylePropertyId.BackgroundImageTileX:
//                case StylePropertyId.BackgroundImageTileY:
//                case StylePropertyId.BackgroundImageOffsetX:
//                case StylePropertyId.BackgroundImageOffsetY:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.BackgroundImage:
                    return DrawTextureAsset(property, isEditable);
                case StylePropertyId.Cursor:
                    return DrawCursor(property, isEditable);

                case StylePropertyId.Opacity:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.GridItemY:
                case StylePropertyId.GridItemHeight:
                case StylePropertyId.GridItemX:
                case StylePropertyId.GridItemWidth:
                    return DrawInt(property, isEditable);

                case StylePropertyId.GridLayoutDirection:
                    return DrawEnumWithValue<LayoutDirection>(property, isEditable);

                case StylePropertyId.GridLayoutDensity:
                    return DrawEnumWithValue<GridLayoutDensity>(property, isEditable);

                case StylePropertyId.GridLayoutColTemplate:
                case StylePropertyId.GridLayoutRowTemplate:
                    return DrawGridTemplate(property, isEditable);

                case StylePropertyId.GridLayoutColAutoSize:
                case StylePropertyId.GridLayoutRowAutoSize:
                    return DrawGridTemplate(property, isEditable);

                case StylePropertyId.GridLayoutColGap:
                case StylePropertyId.GridLayoutRowGap:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.GridLayoutColAlignment:
                case StylePropertyId.GridLayoutRowAlignment:
                    return DrawEnumWithValue<GridAxisAlignment>(property, isEditable);

                case StylePropertyId.FlexLayoutWrap:
                    return DrawEnumWithValue<WrapMode>(property, isEditable);

                case StylePropertyId.FlexLayoutDirection:
                    return DrawEnumWithValue<LayoutDirection>(property, isEditable);

                case StylePropertyId.FlexLayoutMainAxisAlignment:
                    return DrawEnumWithValue<MainAxisAlignment>(property, isEditable);

                case StylePropertyId.FlexLayoutCrossAxisAlignment:
                    return DrawEnumWithValue<CrossAxisAlignment>(property, isEditable);

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
                    return DrawOffsetMeasurement(property, isEditable);

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
                    return DrawEnumWithValue<TransformBehavior>(property, isEditable);

                case StylePropertyId.__TextPropertyStart__:
                case StylePropertyId.__TextPropertyEnd__:
                    break;
                case StylePropertyId.TextColor:
                    return DrawColor(property, isEditable);

                case StylePropertyId.TextFontAsset:
                    return DrawFontAsset(property, isEditable);

                case StylePropertyId.TextFontSize:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.TextFontStyle:
                    // todo -- this needs to be an EnumFlags popup
                    return DrawEnumWithValue<Text.FontStyle>(property, isEditable);
//                    return DrawEnum<Text.FontStyle>(property, isEditable);

                case StylePropertyId.TextAlignment:
                    return DrawEnumWithValue<Text.TextAlignment>(property, isEditable);

                case StylePropertyId.TextWhitespaceMode:
                    return DrawEnumWithValue<WhitespaceMode>(property, isEditable);
//
                case StylePropertyId.TextTransform:
                    return DrawEnumWithValue<TextTransform>(property, isEditable);

                case StylePropertyId.AlignmentBehaviorX:
                case StylePropertyId.AlignmentBehaviorY:
                    return DrawEnumWithValue<AlignmentBehavior>(property, isEditable);
                
                case StylePropertyId.AlignmentDirectionX:
                case StylePropertyId.AlignmentDirectionY:
                    return DrawEnumWithValue<AlignmentDirection>(property, isEditable);
                    
                
                
                case StylePropertyId.AlignmentOffsetX:
                case StylePropertyId.AlignmentOffsetY:
                case StylePropertyId.AlignmentOriginX:
                case StylePropertyId.AlignmentOriginY:
                    return DrawOffsetMeasurement(property, isEditable);
                
                case StylePropertyId.MinWidth:
                case StylePropertyId.MaxWidth:
                case StylePropertyId.PreferredWidth:
                case StylePropertyId.MinHeight:
                case StylePropertyId.MaxHeight:
                case StylePropertyId.PreferredHeight:
                    return DrawMeasurement(property, isEditable);

                case StylePropertyId.LayoutType:
                    return DrawEnumWithValue<LayoutType>(property, isEditable);

                case StylePropertyId.LayoutBehavior:
                    return DrawEnumWithValue<LayoutBehavior>(property, isEditable);

                case StylePropertyId.AnchorTop:
                case StylePropertyId.AnchorRight:
                case StylePropertyId.AnchorBottom:
                case StylePropertyId.AnchorLeft:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.AnchorTarget:
                    return DrawEnumWithValue<AnchorTarget>(property, isEditable);

                case StylePropertyId.ZIndex:
                case StylePropertyId.RenderLayerOffset:
                    return DrawInt(property, isEditable);

                case StylePropertyId.RenderLayer:
                    return DrawEnumWithValue<RenderLayer>(property, isEditable);

//                default:
                //throw new ArgumentOutOfRangeException(property.propertyId.ToString());
            }

            return StyleProperty.Unset(property.propertyId);
        }

        private static StyleProperty DrawCursor(StyleProperty property, bool isEditable) {
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            Texture2D texture = property.AsCursorStyle?.texture;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(StyleUtil.GetPropertyName(property));
            Texture2D newTexture = (Texture2D) EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            // todo fix return value
            return property;
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

        private static StyleProperty DrawEnumWithValue<T>(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            ValueTuple<int[], GUIContent[]> tuple = GetEnumValues<T>();

            int[] values = tuple.Item1;
            GUIContent[] displayOptions = tuple.Item2;
            int index = Array.IndexOf(values, property.int0);
            int output = EditorGUILayout.Popup(s_Content, index, displayOptions);
            // unclear if output is a value or an index, I suspect index
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, values[output]) : property;
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

        private static StyleProperty DrawString(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            string value = EditorGUILayout.TextField(s_Content, property.AsString);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId,  value) : property;
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

        private static StyleProperty DrawOffsetMeasurement(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUILayout.BeginHorizontal();
            GUI.enabled = isEditable;
            float value = EditorGUILayout.FloatField(s_Content, property.AsUIFixedLength.value);
            OffsetMeasurementUnit unit = (OffsetMeasurementUnit) EditorGUILayout.EnumPopup(property.AsUIFixedLength.unit);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, new OffsetMeasurement(value, unit)) : property;
        }

        private static StyleProperty DrawGridTrackSize(StyleProperty property, bool isEditable) {
            GUI.enabled = isEditable;
            s_Content.text = StyleUtil.GetPropertyName(property);
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
            return isEditable ? new StyleProperty(property.propertyId,  newTexture) : property;
        }

        private static StyleProperty DrawFontAsset(StyleProperty property, bool isEditable) {
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            FontAsset fontAsset = property.AsFont;

            TMP_FontAsset newFont = (TMP_FontAsset) EditorGUILayout.ObjectField(StyleUtil.GetPropertyName(property),
                fontAsset.textMeshProFont, typeof(TMP_FontAsset), false);

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId,  default(FontAsset)) : property;
        }

        private static StyleProperty DrawGridTemplate(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            IReadOnlyList<GridTrackSize> template = property.AsGridTrackTemplate;
            EditorGUILayout.BeginHorizontal();
            if (template == null) {
                EditorGUILayout.LabelField("Undefined");
            }
            else {
                EditorGUILayout.LabelField(s_Content);
                for (int i = 0; i < template.Count; i++) {
                    float value = EditorGUILayout.FloatField(template[i].minValue);
                    GridTemplateUnit unit = (GridTemplateUnit) EditorGUILayout.EnumPopup(template[i].minUnit);
                }
            }

            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId,  default(IReadOnlyList<GridTrackSize>)) : property;
        }

    }

    public class StylePropertyIdComparer : IComparer<ValueTuple<string, StyleProperty>> {

        public int Compare(ValueTuple<string, StyleProperty> x, ValueTuple<string, StyleProperty> y) {
            return (int) x.Item2.propertyId > (int) y.Item2.propertyId ? 1 : -1;
        }

    }

}