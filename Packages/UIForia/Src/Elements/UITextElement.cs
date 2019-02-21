using System;
using SVGX;
using TMPro;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;

namespace UIForia {

    // <Paragraph/>
    // <Header1/>
    // <Label/>

    // while parent is text span
    // if parent is something else -> layout injects text container parent
    // 1 text contains many spans
    // span created by default if not created from template
    // text span elements do not lay themselves out or render themselves
    // they contain scoped font properties
    // text span can only contain text (and maybe a shape / sprite)
    // text container can contain non text things, text will be treated as blocks
    // text flow container can handle layouting text to flow around other contents
    // 

    public class UITextElement : UIElement, IMeshProvider, IMaterialProvider {

        public string text;
        public TextInfo textInfo;
        private Mesh mesh;
        private bool meshRequiresUpdate;

        // todo -- see if there is a way to do material sharing, most text can use the same materials
        private Material material;

        private TMP_FontAsset fontAsset;

        public event Action<UITextElement, string> onTextChanged;

        private static readonly Material s_BaseTextMaterial;
        private static readonly int s_TextureSizeKey = Shader.PropertyToID("_TextureSize");
        private static readonly int s_FontScaleKey = Shader.PropertyToID("_FontScale");

        static UITextElement() {
            s_BaseTextMaterial = Resources.Load<Material>("UIForia/Materials/UIForiaText");
        }

        public UITextElement(string text = "") {
            this.text = text ?? string.Empty;
            Renderer = ElementRenderer.DefaultText;
            material = new Material(s_BaseTextMaterial);
            flags = flags | UIElementFlags.TextElement
                          | UIElementFlags.Primitive;
            UpdateTextInfo();
        }

        public override void OnReady() {
            UpdateTextInfo();
        }

        public string GetText() {
            return text;
        }

        public void SetText(string newText) {
            if (this.text == newText) {
                return;
            }

            this.text = newText;

            UpdateTextInfo();
            meshRequiresUpdate = true;

            onTextChanged?.Invoke(this, text);
        }

        public override string GetDisplayName() {
            return "Text";
        }

        private void UpdateTextInfo() {
//            bool collapseSpaces = true; //style.computedStyle.TextCollapseWhiteSpace;
//            bool preserveNewlines = false; //style.computedStyle.TextPreserveNewLines;

            if (textInfo.spanInfos != null) ArrayPool<SpanInfo>.Release(ref textInfo.spanInfos);
            if (textInfo.wordInfos != null) ArrayPool<WordInfo>.Release(ref textInfo.wordInfos);
            if (textInfo.lineInfos != null) ArrayPool<LineInfo>.Release(ref textInfo.lineInfos);
            if (textInfo.charInfos != null) ArrayPool<CharInfo>.Release(ref textInfo.charInfos);

//            textInfo = TextUtil.ProcessText(text, collapseSpaces, preserveNewlines, style.TextTransform);
//            textInfo.spanCount = 1;
//            textInfo.spanInfos = ArrayPool<SpanInfo>.GetMinSize(1);
//            textInfo.spanInfos[0].wordCount = textInfo.wordCount;
//            textInfo.spanInfos[0].font = style.TextFontAsset;
//            textInfo.spanInfos[0].charCount = textInfo.charCount;
//            textInfo.spanInfos[0].fontSize = style.TextFontSize;
//            textInfo.spanInfos[0].fontStyle = style.TextFontStyle;
//            textInfo.spanInfos[0].alignment = style.TextAlignment;

            textInfo = TextUtil.CreateTextInfo(new TextUtil.TextSpan(style.TextFontAsset, new SVGXTextStyle(), text));
            
//            ComputeCharacterAndWordSizes(textInfo);
        }

        public SelectionRange AppendText(char character) {
            SetText(text + character);
            return new SelectionRange(textInfo.charCount - 1, TextEdge.Right);
        }

        public SelectionRange AppendText(string str) {
            SetText(text + str);
            return new SelectionRange(textInfo.charCount - 1, TextEdge.Right);
        }

        public SelectionRange DeleteTextBackwards(SelectionRange range) {
            if (text.Length == 0) {
                return range;
            }

            int cursorIndex = Mathf.Clamp(range.cursorIndex, 0, textInfo.charCount - 1);
            if (range.HasSelection) {
                int min = (range.cursorIndex < range.selectIndex ? range.cursorIndex : range.selectIndex) + 1;
                int max = (range.cursorIndex > range.selectIndex ? range.cursorIndex : range.selectIndex) + 1;

                string part0 = text.Substring(0, min);
                string part1 = text.Substring(max);
                SetText(part0 + part1);
                return new SelectionRange(min, TextEdge.Left);
            }
            else {
                if (cursorIndex == 0 && range.cursorEdge == TextEdge.Left) {
                    return range;
                }

                // assume same line for the moment
                if (range.cursorEdge == TextEdge.Left) {
                    cursorIndex--;
                }

                cursorIndex = Mathf.Max(0, cursorIndex);

                if (cursorIndex == 0) {
                    SetText(text.Substring(1));
                    return new SelectionRange(0, TextEdge.Left);
                }
                else if (cursorIndex == textInfo.charCount - 1) {
                    SetText(text.Substring(0, text.Length - 1));
                    return new SelectionRange(range.cursorIndex - 1, TextEdge.Right);
                }
                else {
                    string part0 = text.Substring(0, cursorIndex);
                    string part1 = text.Substring(cursorIndex + 1);
                    SetText(part0 + part1);

                    return new SelectionRange(cursorIndex - 1, TextEdge.Right);
                }
            }
        }

        public SelectionRange DeleteTextForwards(SelectionRange range) {
            if (text.Length == 0) {
                return range;
            }

            int cursorIndex = Mathf.Clamp(range.cursorIndex, 0, textInfo.charCount - 1);
            if (range.HasSelection) {
                int min = (range.cursorIndex < range.selectIndex ? range.cursorIndex : range.selectIndex) + 1;
                int max = (range.cursorIndex > range.selectIndex ? range.cursorIndex : range.selectIndex) + 1;

                string part0 = text.Substring(0, min);
                string part1 = text.Substring(max);
                SetText(part0 + part1);
                return new SelectionRange(min, TextEdge.Left);
            }
            else {
                if (cursorIndex == textInfo.charCount - 1 && range.cursorEdge == TextEdge.Right) {
                    return range;
                }
                else {
                    if (cursorIndex == textInfo.charCount - 1) {
                        SetText(text.Remove(textInfo.charCount - 1));
                        return new SelectionRange(textInfo.charCount - 1, TextEdge.Right);
                    }
                    else {
                        string part0 = text.Substring(0, cursorIndex + 1);
                        string part1 = text.Substring(cursorIndex + 2);
                        SetText(part0 + part1);
                        return new SelectionRange(cursorIndex, TextEdge.Right);
                    }
                }
            }
        }

        public SelectionRange MoveCursorLeft(SelectionRange range, bool maintainSelection) {
            int selectionIndex = range.selectIndex;
            TextEdge selectionEdge = range.selectEdge;

            if (!maintainSelection && range.HasSelection) {
                selectionIndex = -1;
                return new SelectionRange(range.cursorIndex, range.cursorEdge);
            }
            else if (!maintainSelection) {
                selectionIndex = -1;
            }
            else if (!range.HasSelection) {
                selectionIndex = range.cursorIndex;
                selectionEdge = range.cursorEdge;
            }

            if (range.cursorEdge == TextEdge.Left) {
                if (range.cursorIndex == 0) {
                    return new SelectionRange(range.cursorIndex, range.cursorEdge, selectionIndex, selectionEdge);
                }

                return new SelectionRange(Mathf.Max(0, range.cursorIndex - 2), TextEdge.Right, selectionIndex, selectionEdge);
            }

            if (range.cursorIndex - 1 < 0) {
                return new SelectionRange(0, TextEdge.Left, selectionIndex, selectionEdge);
            }

            return new SelectionRange(range.cursorIndex - 1, TextEdge.Right, selectionIndex, selectionEdge);
        }

        public SelectionRange MoveCursorRight(SelectionRange range, bool maintainSelection) {
            int selectionIndex = range.selectIndex;
            TextEdge selectionEdge = range.selectEdge;

            if (!maintainSelection && range.HasSelection) {
                selectionIndex = -1;
                return new SelectionRange(range.cursorIndex, range.cursorEdge);
            }
            else if (!maintainSelection) {
                selectionIndex = -1;
            }
            else if (!range.HasSelection) {
                selectionIndex = range.cursorIndex;
                selectionEdge = range.cursorEdge;
            }

            if (range.cursorEdge == TextEdge.Left) {
                return new SelectionRange(range.cursorIndex, TextEdge.Right, selectionIndex, selectionEdge);
            }

            int cursorIndex = Mathf.Min(range.cursorIndex + 1, textInfo.charCount - 1);

            if (cursorIndex == textInfo.charCount - 1) {
                return new SelectionRange(cursorIndex, TextEdge.Right, selectionIndex, selectionEdge);
            }

            return new SelectionRange(cursorIndex, TextEdge.Right, selectionIndex, selectionEdge);
        }

        public void InsertText(SelectionRange range, char character) { }

        public void InsertText(int characterIndex, string str) { }

    
      

       
        public Mesh GetMesh() {
            if (!meshRequiresUpdate && (mesh != null && !layoutResult.SizeChanged)) {
                return mesh;
            }

            if (mesh == null) mesh = new Mesh();
            mesh.Clear();
            meshRequiresUpdate = false;

            TextUtil.ApplyLineAndWordOffsets(textInfo);

            CharInfo[] charInfos = textInfo.charInfos;

            int sizeX4 = textInfo.charCount * 4;

            int[] triangles = new int[textInfo.charCount * 6];
            Vector3[] positions = new Vector3[textInfo.charCount * 4];
            Vector2[] uvs0 = new Vector2[sizeX4];
            Vector2[] uvs2 = new Vector2[sizeX4];
            Vector3[] normals = new Vector3[sizeX4];
            Vector4[] tangents = new Vector4[sizeX4];
            Color32[] colors = new Color32[sizeX4];

            Color32 color = style.TextColor;

            int idx_x4 = 0;
            int idx_x6 = 0;

            Size actualSize = layoutResult.actualSize;

            // todo -- convert this to the job system
            for (int i = 0; i < textInfo.charCount; i++) {
                if (charInfos[i].character == ' ') continue;

                for (int j = 0; j < 4; j++) {
                    normals[idx_x4 + j] = Vector3.back;
                    tangents[idx_x4 + j] = new Vector4(-1f, 0, 0, 1f);
                    colors[idx_x4 + j] = color;
                }

                Vector2 topLeft = charInfos[i].topLeft;
                Vector2 bottomRight = charInfos[i].bottomRight;

                positions[idx_x4 + 0] = new Vector3(topLeft.x, -bottomRight.y, 0); // Bottom Left
                positions[idx_x4 + 1] = new Vector3(topLeft.x, -topLeft.y, 0); // Top Left
                positions[idx_x4 + 2] = new Vector3(bottomRight.x, -topLeft.y, 0); // Top Right
                positions[idx_x4 + 3] = new Vector3(bottomRight.x, -bottomRight.y); // Bottom Right

                uvs0[idx_x4 + 0] = new Vector2(charInfos[i].uv0.x, charInfos[i].uv0.y);
                uvs0[idx_x4 + 1] = new Vector2(charInfos[i].uv0.x, charInfos[i].uv1.y);
                uvs0[idx_x4 + 2] = new Vector2(charInfos[i].uv1.x, charInfos[i].uv1.y);
                uvs0[idx_x4 + 3] = new Vector2(charInfos[i].uv1.x, charInfos[i].uv0.y);

                float leftClipX = topLeft.x / actualSize.width;
                float rightClipX = bottomRight.x / actualSize.width;
                float topClipY = topLeft.y / actualSize.height;
                float bottomClipY = bottomRight.y / actualSize.height;

                uvs2[idx_x4 + 0] = new Vector2(leftClipX, bottomClipY);
                uvs2[idx_x4 + 1] = new Vector2(leftClipX, topClipY);
                uvs2[idx_x4 + 2] = new Vector2(rightClipX, topClipY);
                uvs2[idx_x4 + 3] = new Vector2(rightClipX, bottomClipY);

                triangles[idx_x6 + 0] = idx_x4 + 0;
                triangles[idx_x6 + 1] = idx_x4 + 1;
                triangles[idx_x6 + 2] = idx_x4 + 2;
                triangles[idx_x6 + 3] = idx_x4 + 2;
                triangles[idx_x6 + 4] = idx_x4 + 3;
                triangles[idx_x6 + 5] = idx_x4 + 0;

                idx_x4 += 4;
                idx_x6 += 6;
            }

            mesh.vertices = positions;
            mesh.uv = uvs0;
            mesh.uv2 = uvs2;
            mesh.colors32 = colors;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.triangles = triangles;

            return mesh;
        }

        public Material GetMaterial() {
            Material fontMaterial = style.TextFontAsset.material;

          //  if (fontAsset != style.TextFontAsset) {
                fontAsset = style.TextFontAsset;
                material.mainTexture = fontMaterial.mainTexture;
                material.SetFloat(ShaderUtilities.ID_GradientScale, fontMaterial.GetFloat(ShaderUtilities.ID_GradientScale));
                material.SetFloat(ShaderUtilities.ID_WeightNormal, fontMaterial.GetFloat(ShaderUtilities.ID_WeightNormal));
                material.SetFloat(ShaderUtilities.ID_WeightBold, fontMaterial.GetFloat(ShaderUtilities.ID_WeightBold));
                material.SetFloat(ShaderUtilities.ID_ScaleRatio_A, fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A));
                material.SetFloat(ShaderUtilities.ID_ScaleRatio_B, fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_B));
                material.SetFloat(ShaderUtilities.ID_ScaleRatio_C, fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A));
                material.SetVector(s_TextureSizeKey, new Vector4(material.mainTexture.width, material.mainTexture.height));
           // }

            float fontScale = (style.TextFontSize / fontAsset.fontInfo.PointSize) * fontAsset.fontInfo.Scale;
            ;
            material.SetFloat(s_FontScaleKey, fontScale);
            // todo -- text styles & keywords, try to use the same materials where possible
//            material.SetVector("_OutlineColor", style.TextOutlineColor);
//            material.SetVector("_OutlineSettings", new Vector4(computedStyle.TextOutlineWidth, computedStyle.TextOutlineSoftness));
//            
//            Vector4 glowSettings = new Vector4(
//                computedStyle.TextGlowInnerSize, 
//                computedStyle.TextGlowOuterSize,
//                computedStyle.TextGlowPower,
//                PackFloat(computedStyle.TextGlowOffset)
//            );
//            
//            material.SetVector("_GlowColor", computedStyle.TextGlowColor);
//            material.SetVector("_GlowColorSettings", glowSettings);
//
//            material.SetVector("_UnderlaySettings", underlaySettings);
//            material.SetVector("_UnderlayColor", underlayColor);
//            
//            material.SetTexture("_FaceTexture", computedStyle.TextFaceTexture);

            return material;
        }

        private WordInfo GetWordAtPoint(Vector2 point) {
            return FindNearestWord(FindNearestLine(point), point);
        }

        public SelectionRange DeleteRange(SelectionRange selectionRange) {
            return new SelectionRange();
        }

        public string GetSubstring(SelectionRange selectionRange) {
            if (!selectionRange.HasSelection) {
                return string.Empty;
            }

            int start = Mathf.Min(selectionRange.cursorIndex, selectionRange.selectIndex);
            int end = Mathf.Max(selectionRange.cursorIndex, selectionRange.selectIndex);

            char[] chars = new char[end - start];
            int idx = 0;
            for (int i = start; i < end; i++) {
                chars[idx++] = textInfo.charInfos[i].character;
            }

            return new string(chars);
        }

        public SelectionRange SelectAll() {
            return new SelectionRange(textInfo.charCount - 1, TextEdge.Right, 0, TextEdge.Left);
        }

        private TextEdge FindCursorEdge(int charIndex, Vector2 point) {
            if (string.IsNullOrEmpty(text) || charIndex > text.Length - 1) {
                return TextEdge.Left;
            }

            CharInfo charInfo = textInfo.charInfos[charIndex];
            float width = charInfo.bottomRight.x - charInfo.topLeft.x;
            if (point.x > charInfo.topLeft.x + (width * 0.5f)) {
                return TextEdge.Right;
            }

            return TextEdge.Left;
        }

        private int FindNearestCharacterIndex(WordInfo wordInfo, Vector2 point) {
            int closestIndex = wordInfo.startChar;
            float closestDistance = float.MaxValue;
            CharInfo[] charInfos = textInfo.charInfos;

            for (int i = wordInfo.startChar; i < wordInfo.startChar + wordInfo.charCount; i++) {
                CharInfo charInfo = charInfos[i];
                float x1 = charInfo.topLeft.x;
                float x2 = charInfo.bottomRight.x;

                if (point.x >= x1 && point.x <= x2) {
                    return i;
                }

                float distToY1 = Mathf.Abs(point.x - x1);
                float distToY2 = Mathf.Abs(point.x - x2);
                if (distToY1 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY1;
                }

                if (distToY2 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY2;
                }
            }

            return closestIndex;
        }

        private int FindNearestCharacterIndex(Vector2 point) {
            LineInfo nearestLine = FindNearestLine(point);
            WordInfo nearestWord = FindNearestWord(nearestLine, point);
            return FindNearestCharacterIndex(nearestWord, point);
        }

        private LineInfo FindNearestLine(Vector2 point) {
            return textInfo.lineInfos[FindNearestLineIndex(point)];
        }

        public float GetLineHeight() {
            TMP_FontAsset asset = style.TextFontAsset;
            float scale = (style.TextFontSize / asset.fontInfo.PointSize) * asset.fontInfo.Scale;
            return (asset.fontInfo.Ascender - asset.fontInfo.Descender) * scale;
        }

        private int FindNearestLineIndex(Vector2 point) {
            float lh = GetLineHeight();
            if (point.y <= textInfo.lineInfos[0].position.y) {
                return 0;
            }

            if (point.y >= textInfo.lineInfos[textInfo.lineInfos.Length - 1].position.y) {
                return textInfo.lineInfos.Length - 1;
            }

            for (int i = 0; i < textInfo.lineCount; i++) {
                LineInfo line = textInfo.lineInfos[i];

                if (line.position.y <= point.y && line.position.y + lh >= point.y) {
                    return i;
                }
            }


            return 0; // should never reach this
//            float closestDistance = float.MaxValue;
//            for (int i = 0; i < textInfo.lineCount; i++) {
//                LineInfo line = textInfo.lineInfos[i];
//                float y1 = -line.maxAscender;
//                float y2 = -line.maxDescender;
//
//                if (point.y >= y1 && point.y <= y2) {
//                    return i;
//                }
//
//                float distToY1 = Mathf.Abs(point.y - y1);
//                float distToY2 = Mathf.Abs(point.y - y2);
//                if (distToY1 < closestDistance) {
//                    closestIndex = i;
//                    closestDistance = distToY1;
//                }
//
//                if (distToY2 < closestDistance) {
//                    closestIndex = i;
//                    closestDistance = distToY2;
//                }
//            }
//
//            return closestIndex;
        }

        private WordInfo FindNearestWord(LineInfo line, Vector2 point) {
            int closestIndex = 0;
            float closestDistance = float.MaxValue;
            for (int i = line.wordStart; i < line.wordStart + line.wordCount; i++) {
                WordInfo word = textInfo.wordInfos[i];
                float x1 = word.position.x;
                float x2 = word.position.x + word.xAdvance;
                if (point.x >= x1 && point.x <= x2) {
                    return word;
                }

                float distToX1 = Mathf.Abs(point.x - x1);
                float distToX2 = Mathf.Abs(point.x - x2);
                if (distToX1 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToX1;
                }

                if (distToX2 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToX2;
                }
            }

            return textInfo.wordInfos[closestIndex];
        }

        public Mesh GetHighlightMesh(SelectionRange selectionRange, Mesh highlightMesh = null) {
            if (string.IsNullOrEmpty(text)) {
                return null;
            }

            Color32 selectionColor = new Color32(168, 206, 255, 192);

            int startIndex = selectionRange.selectIndex;
            int endIndex = selectionRange.cursorIndex;
            TextEdge startEdge = selectionRange.selectEdge;
            TextEdge endEdge = selectionRange.cursorEdge;

            if (selectionRange.cursorIndex < selectionRange.selectIndex) {
                startIndex = selectionRange.cursorIndex;
                endIndex = selectionRange.selectIndex;
                startEdge = selectionRange.cursorEdge;
                endEdge = selectionRange.selectEdge;
            }

            CharInfo startCharInfo = textInfo.charInfos[startIndex];
            CharInfo endCharInfo = textInfo.charInfos[endIndex];
            highlightMesh = highlightMesh ? highlightMesh : new Mesh();

            // todo -- needs to handle all lines not just first
            float height = GetLineHeight();
            if (startCharInfo.lineIndex == endCharInfo.lineIndex) {
                // what space is this in?
                LineInfo currentLine = textInfo.lineInfos[startCharInfo.lineIndex];
                float minX = startEdge == TextEdge.Right ? startCharInfo.bottomRight.x : startCharInfo.topLeft.x;
                float maxX = endEdge == TextEdge.Right ? endCharInfo.bottomRight.x : endCharInfo.topLeft.x;
                float minY = currentLine.position.y - layoutResult.localPosition.y;
                float maxY = currentLine.position.y - height - layoutResult.localPosition.y;
                Vector3 v0 = new Vector3(minX, minY);
                Vector3 v1 = new Vector3(minX, maxY);
                Vector3 v2 = new Vector3(maxX, maxY);
                Vector3 v3 = new Vector3(maxX, minY);

                MeshUtil.s_VertexHelper.AddVert(v0, selectionColor, new Vector2(0, 1));
                MeshUtil.s_VertexHelper.AddVert(v1, selectionColor, new Vector2(0, 0));
                MeshUtil.s_VertexHelper.AddVert(v2, selectionColor, new Vector2(1, 0));
                MeshUtil.s_VertexHelper.AddVert(v3, selectionColor, new Vector2(1, 1));

                MeshUtil.s_VertexHelper.AddTriangle(0, 1, 2);
                MeshUtil.s_VertexHelper.AddTriangle(2, 3, 0);


                MeshUtil.s_VertexHelper.FillMesh(highlightMesh);
                MeshUtil.s_VertexHelper.Clear();

                return highlightMesh;
            }

            int lineCount = endCharInfo.lineIndex = startCharInfo.lineIndex;
            Debug.Log("Multi line");
            for (int i = startCharInfo.lineIndex; i < lineCount; i++) { }

            return highlightMesh;
        }

        public SelectionRange GetSelectionAtPoint(Vector2 point) {
            if (string.IsNullOrEmpty(text)) {
                return new SelectionRange(0, TextEdge.Left, -1, TextEdge.Left);
            }

            int charIndex = FindNearestCharacterIndex(point);
            return new SelectionRange(charIndex, FindCursorEdge(charIndex, point));
        }

        public SelectionRange SelectWordAtPoint(Vector2 point) {
            WordInfo wordInfo = GetWordAtPoint(point);
            return new SelectionRange(
                wordInfo.startChar + wordInfo.VisibleCharCount - 1,
                TextEdge.Right,
                wordInfo.startChar
            );
        }

        public SelectionRange ValidateSelectionRange(SelectionRange range) {
            int cursorIdx = (range.cursorIndex < textInfo.charCount) ? range.cursorIndex : textInfo.charCount - 1;
            int selectIdx = (range.selectIndex < textInfo.charCount) ? range.selectIndex : textInfo.charCount - 1;
            return new SelectionRange(cursorIdx, range.cursorEdge, selectIdx, range.selectEdge);
        }

        public SelectionRange SelectToPoint(SelectionRange range, Vector2 point) {
            int charIndex = FindNearestCharacterIndex(point);
            return new SelectionRange(
                charIndex,
                FindCursorEdge(charIndex, point),
                range.selectIndex,
                range.selectEdge
            );
        }

        public Mesh GetCursorMesh(SelectionRange selectionRange, float cursorWidth = 1f, Mesh cursorMesh = null) {
            if (cursorMesh == null) {
                cursorMesh = new Mesh();
            }

            if (textInfo.charCount == 0) { }

            CharInfo info = textInfo.charInfos[selectionRange.cursorIndex];
            TextEdge edge = selectionRange.cursorEdge;
            LineInfo currentLine = textInfo.lineInfos[info.lineIndex];

            float minX = edge == TextEdge.Left ? info.topLeft.x : info.bottomRight.x;
            float maxX = minX + cursorWidth;
            float minY = currentLine.position.y - layoutResult.localPosition.y;
            float maxY = currentLine.position.y - GetLineHeight() - layoutResult.localPosition.y;
            Vector3 v0 = new Vector3(minX, minY);
            Vector3 v1 = new Vector3(minX, maxY);
            Vector3 v2 = new Vector3(maxX, maxY);
            Vector3 v3 = new Vector3(maxX, minY);

            MeshUtil.s_VertexHelper.AddVert(v0, new Color32(), new Vector2(0, 1));
            MeshUtil.s_VertexHelper.AddVert(v1, new Color32(), new Vector2(0, 0));
            MeshUtil.s_VertexHelper.AddVert(v2, new Color32(), new Vector2(1, 0));
            MeshUtil.s_VertexHelper.AddVert(v3, new Color32(), new Vector2(1, 1));

            MeshUtil.s_VertexHelper.AddTriangle(0, 1, 2);
            MeshUtil.s_VertexHelper.AddTriangle(2, 3, 0);

            MeshUtil.s_VertexHelper.FillMesh(cursorMesh);
            MeshUtil.s_VertexHelper.Clear();
            return cursorMesh;
        }

        public enum TextEdge {

            Right,
            Left

        }

        public struct SelectionRange {

            public readonly int cursorIndex;
            public readonly int selectIndex;
            public readonly TextEdge cursorEdge;
            public readonly TextEdge selectEdge;

            public SelectionRange(int cursorIndex, TextEdge cursorEdge, int selectIndex = -1,
                TextEdge selectEdge = TextEdge.Left) {
                this.cursorIndex = cursorIndex;
                this.cursorEdge = cursorEdge;
                this.selectIndex = selectIndex;
                this.selectEdge = selectEdge;
            }

            public bool HasSelection => selectIndex != -1 && selectIndex != cursorIndex;

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                return obj is SelectionRange && Equals((SelectionRange) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = cursorIndex;
                    hashCode = (hashCode * 397) ^ selectIndex;
                    hashCode = (hashCode * 397) ^ (int) cursorEdge;
                    hashCode = (hashCode * 397) ^ (int) selectEdge;
                    return hashCode;
                }
            }

            public bool Equals(SelectionRange previousSelectionRange) {
                return cursorIndex != previousSelectionRange.cursorIndex
                       || cursorEdge != previousSelectionRange.cursorEdge
                       || selectEdge != previousSelectionRange.selectEdge
                       || selectIndex != previousSelectionRange.selectIndex;
            }

            public static bool operator ==(SelectionRange a, SelectionRange b) {
                return a.cursorIndex == b.cursorIndex
                       && a.cursorEdge == b.cursorEdge
                       && a.selectEdge == b.selectEdge
                       && a.selectIndex == b.selectIndex;
            }

            public static bool operator !=(SelectionRange a, SelectionRange b) {
                return !(a == b);
            }

        }
        
        public Vector2 GetCursorPosition(SelectionRange selectionRange) {
            if (string.IsNullOrEmpty(text) || selectionRange.cursorIndex >= textInfo.charCount) {
                return Vector2.zero;
            }
            
            CharInfo charInfo = textInfo.charInfos[selectionRange.cursorIndex];
            LineInfo lineInfo = textInfo.lineInfos[charInfo.lineIndex];
            
            return new Vector2(selectionRange.cursorEdge == TextEdge.Right
                    ? charInfo.bottomRight.x
                    : charInfo.topLeft.x,
                lineInfo.position.y
            );
        }

        public SelectionRange BeginSelection(Vector2 point) {
            int selectIdx = FindNearestCharacterIndex(point);
            TextEdge selectEdge = FindCursorEdge(selectIdx, point);
            return new SelectionRange(selectIdx, selectEdge, selectIdx, selectEdge);
        }

    }

}