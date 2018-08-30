using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Debugger {

    public class GOInputFieldRender : MonoBehaviour, ICanvasElement {

        protected Graphic m_Placeholder;
        private Text m_TextComponent;
        private TextGenerator m_InputTextCache;
        private CanvasRenderer m_CachedInputRenderer;
        private RectTransform caretRectTrans;
        private Mesh m_Mesh;
        private UIVertex[] m_CursorVertices;
        private int m_CaretPosition = 0;
        private int m_CaretSelectPosition = 0;
        protected bool m_CaretVisible;
        protected int m_DrawStart = 0;
        protected int m_DrawEnd = 0;
        private bool m_AllowInput = false;
        private Color selectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);
        private bool multiLine;
        protected string m_Text = string.Empty;
        private bool m_PreventFontCallback;
        private Coroutine m_BlinkCoroutine = null;
        private float m_BlinkStartTime = 0.0f;
        private float m_CaretBlinkRate = 0.85f;
        public bool isFocused;
        public bool hasSelection;
        
        private Mesh mesh {
            get {
                m_Mesh = m_Mesh != null ? new Mesh() : m_Mesh;
                return m_Mesh;
            }
        }

        protected TextGenerator cachedInputTextGenerator {
            get {
                if (m_InputTextCache == null)
                    m_InputTextCache = new TextGenerator();

                return m_InputTextCache;
            }
        }

        protected int caretPositionInternal {
            get { return m_CaretPosition + Input.compositionString.Length; }
            set {
                m_CaretPosition = value;
                ClampPos(ref m_CaretPosition);
            }
        }

        protected int caretSelectPositionInternal {
            get { return m_CaretSelectPosition + Input.compositionString.Length; }
            set {
                m_CaretSelectPosition = value;
                ClampPos(ref m_CaretSelectPosition);
            }
        }

        public string text {
            get { return m_Text; }
            set {
                if (this.text == value)
                    return;

                m_Text = value;
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    SendOnValueChangedAndUpdateLabel();
                    return;
                }
#endif
                if (m_CaretPosition > m_Text.Length)
                    m_CaretPosition = m_CaretSelectPosition = m_Text.Length;

                SendOnValueChangedAndUpdateLabel();
            }
        }


        public void Rebuild(CanvasUpdate update) {
            if (update == CanvasUpdate.LatePreRender) {
                UpdateGeometry();
            }
        }

        public void LayoutComplete() { }

        public void GraphicUpdateComplete() { }

        public bool IsDestroyed() {
            return false;
        }

        private void MarkGeometryAsDirty() {
            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        private void UpdateGeometry() {

            if (m_CachedInputRenderer == null && m_TextComponent != null) {
                GameObject go = new GameObject(transform.name + " Input Caret");
                go.hideFlags = HideFlags.DontSave;
                go.transform.SetParent(m_TextComponent.transform.parent);
                go.transform.SetAsFirstSibling();
                go.layer = gameObject.layer;

                caretRectTrans = go.AddComponent<RectTransform>();
                m_CachedInputRenderer = go.AddComponent<CanvasRenderer>();
                m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);
                AssignPositioningIfNeeded();
            }

            if (m_CachedInputRenderer == null) {
                return;
            }

            OnFillVBO(mesh);
            m_CachedInputRenderer.SetMesh(mesh);
        }

        private void SendOnValueChangedAndUpdateLabel() {
            SendOnValueChanged();
            UpdateLabel();
        }

        private void SendOnValueChanged() {
            //  if (onValueChange != null)
            //      onValueChange.Invoke(text);
        }

        private void OnFillVBO(Mesh vbo) {
            using (var helper = new VertexHelper()) {
                if (!isFocused) {
                    helper.FillMesh(vbo);
                    return;
                }

                Rect inputRect = m_TextComponent.rectTransform.rect;
                Vector2 extents = inputRect.size;

                // get the text alignment anchor point for the text in local space
                Vector2 textAnchorPivot = Text.GetTextAnchorPivot(m_TextComponent.alignment);
                Vector2 refPoint = Vector2.zero;
                refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
                refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

                // Adjust the anchor point in screen space
                Vector2 roundedRefPoint = m_TextComponent.PixelAdjustPoint(refPoint);

                // Determine fraction of pixel to offset text mesh.
                // This is the rounding in screen space, plus the fraction of a pixel the text anchor pivot is from the corner of the text mesh.
                Vector2 roundingOffset = roundedRefPoint - refPoint + Vector2.Scale(extents, textAnchorPivot);
                roundingOffset.x = roundingOffset.x - Mathf.Floor(0.5f + roundingOffset.x);
                roundingOffset.y = roundingOffset.y - Mathf.Floor(0.5f + roundingOffset.y);

                if (!hasSelection)
                    GenerateCursor(helper, roundingOffset);
                else
                    GenerateHighlight(helper, roundingOffset);

                helper.FillMesh(vbo);
            }
        }

        private void AssignPositioningIfNeeded() {
            if (m_TextComponent != null && caretRectTrans != null &&
                (caretRectTrans.localPosition != m_TextComponent.rectTransform.localPosition ||
                 caretRectTrans.localRotation != m_TextComponent.rectTransform.localRotation ||
                 caretRectTrans.localScale != m_TextComponent.rectTransform.localScale ||
                 caretRectTrans.anchorMin != m_TextComponent.rectTransform.anchorMin ||
                 caretRectTrans.anchorMax != m_TextComponent.rectTransform.anchorMax ||
                 caretRectTrans.anchoredPosition != m_TextComponent.rectTransform.anchoredPosition ||
                 caretRectTrans.sizeDelta != m_TextComponent.rectTransform.sizeDelta ||
                 caretRectTrans.pivot != m_TextComponent.rectTransform.pivot)) {
                caretRectTrans.localPosition = m_TextComponent.rectTransform.localPosition;
                caretRectTrans.localRotation = m_TextComponent.rectTransform.localRotation;
                caretRectTrans.localScale = m_TextComponent.rectTransform.localScale;
                caretRectTrans.anchorMin = m_TextComponent.rectTransform.anchorMin;
                caretRectTrans.anchorMax = m_TextComponent.rectTransform.anchorMax;
                caretRectTrans.anchoredPosition = m_TextComponent.rectTransform.anchoredPosition;
                caretRectTrans.sizeDelta = m_TextComponent.rectTransform.sizeDelta;
                caretRectTrans.pivot = m_TextComponent.rectTransform.pivot;
            }
        }

        private void ClampPos(ref int pos) {
            if (pos < 0) pos = 0;
            else if (pos > text.Length) pos = text.Length;
        }

        private void GenerateCursor(VertexHelper vbo, Vector2 roundingOffset) {
            if (!m_CaretVisible)
                return;

            if (m_CursorVertices == null) {
                CreateCursorVertices();
            }

            float width = 1f;
            float height = m_TextComponent.fontSize;
            int adjustedPos = Mathf.Max(0, caretPositionInternal - m_DrawStart);
            TextGenerator gen = m_TextComponent.cachedTextGenerator;

            if (gen == null)
                return;

            if (m_TextComponent.resizeTextForBestFit)
                height = gen.fontSizeUsedForBestFit / m_TextComponent.pixelsPerUnit;

            Vector2 startPosition = Vector2.zero;

            // Calculate startPosition
            if (gen.characterCountVisible + 1 > adjustedPos || adjustedPos == 0) {
                UICharInfo cursorChar = gen.characters[adjustedPos];
                startPosition.x = cursorChar.cursorPos.x;
                startPosition.y = cursorChar.cursorPos.y;
            }

            startPosition.x /= m_TextComponent.pixelsPerUnit;

            // TODO: Only clamp when Text uses horizontal word wrap.
            if (startPosition.x > m_TextComponent.rectTransform.rect.xMax) {
                startPosition.x = m_TextComponent.rectTransform.rect.xMax;
            }

            int characterLine = DetermineCharacterLine(adjustedPos, gen);
            float lineHeights = SumLineHeights(characterLine, gen);
            startPosition.y = m_TextComponent.rectTransform.rect.yMax - lineHeights / m_TextComponent.pixelsPerUnit;

            m_CursorVertices[0].position = new Vector3(startPosition.x, startPosition.y - height, 0.0f);
            m_CursorVertices[1].position = new Vector3(startPosition.x + width, startPosition.y - height, 0.0f);
            m_CursorVertices[2].position = new Vector3(startPosition.x + width, startPosition.y, 0.0f);
            m_CursorVertices[3].position = new Vector3(startPosition.x, startPosition.y, 0.0f);

            if (roundingOffset != Vector2.zero) {
                for (int i = 0; i < m_CursorVertices.Length; i++) {
                    UIVertex uiv = m_CursorVertices[i];
                    uiv.position.x += roundingOffset.x;
                    uiv.position.y += roundingOffset.y;
                }
            }

            vbo.AddUIVertexQuad(m_CursorVertices);

            startPosition.y = Screen.height - startPosition.y;
            Input.compositionCursorPos = startPosition;
        }

        private void GenerateHighlight(VertexHelper vbo, Vector2 roundingOffset) {
            int startChar = Mathf.Max(0, caretPositionInternal - m_DrawStart);
            int endChar = Mathf.Max(0, caretSelectPositionInternal - m_DrawStart);

            // Ensure pos is always less then selPos to make the code simpler
            if (startChar > endChar) {
                int temp = startChar;
                startChar = endChar;
                endChar = temp;
            }

            endChar -= 1;
            TextGenerator gen = m_TextComponent.cachedTextGenerator;

            int currentLineIndex = DetermineCharacterLine(startChar, gen);
            float height = m_TextComponent.fontSize;

            if (m_TextComponent.resizeTextForBestFit)
                height = gen.fontSizeUsedForBestFit / m_TextComponent.pixelsPerUnit;

            if (cachedInputTextGenerator != null && cachedInputTextGenerator.lines.Count > 0) {
                // TODO: deal with multiple lines with different line heights.
                height = cachedInputTextGenerator.lines[0].height;
            }

            if (m_TextComponent.resizeTextForBestFit && cachedInputTextGenerator != null) {
                height = cachedInputTextGenerator.fontSizeUsedForBestFit;
            }

            int nextLineStartIdx = GetLineEndPosition(gen, currentLineIndex);

            UIVertex vert = UIVertex.simpleVert;
            vert.uv0 = Vector2.zero;
            vert.color = selectionColor;

            int currentChar = startChar;
            while (currentChar <= endChar && currentChar < gen.characterCountVisible) {
                if (currentChar + 1 == nextLineStartIdx || currentChar == endChar) {
                    UICharInfo startCharInfo = gen.characters[startChar];
                    UICharInfo endCharInfo = gen.characters[currentChar];
                    float lineHeights = SumLineHeights(currentLineIndex, gen);
                    Vector2 startPosition = new Vector2(startCharInfo.cursorPos.x / m_TextComponent.pixelsPerUnit,
                        m_TextComponent.rectTransform.rect.yMax - (lineHeights / m_TextComponent.pixelsPerUnit));
                    Vector2 endPosition = new Vector2((endCharInfo.cursorPos.x + endCharInfo.charWidth) / m_TextComponent.pixelsPerUnit,
                        startPosition.y - height / m_TextComponent.pixelsPerUnit);

                    // Checking xMin as well due to text generator not setting possition if char is not rendered.
                    if (endPosition.x > m_TextComponent.rectTransform.rect.xMax || endPosition.x < m_TextComponent.rectTransform.rect.xMin)
                        endPosition.x = m_TextComponent.rectTransform.rect.xMax;

                    var startIndex = vbo.currentVertCount;
                    vert.position = new Vector3(startPosition.x, endPosition.y, 0.0f) + (Vector3) roundingOffset;
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, endPosition.y, 0.0f) + (Vector3) roundingOffset;
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, startPosition.y, 0.0f) + (Vector3) roundingOffset;
                    vbo.AddVert(vert);

                    vert.position = new Vector3(startPosition.x, startPosition.y, 0.0f) + (Vector3) roundingOffset;
                    vbo.AddVert(vert);

                    vbo.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                    vbo.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);

                    startChar = currentChar + 1;
                    currentLineIndex++;

                    nextLineStartIdx = GetLineEndPosition(gen, currentLineIndex);
                }

                currentChar++;
            }
        }


        private int DetermineCharacterLine(int charPos, TextGenerator generator) {
            if (!multiLine)
                return 0;

            for (int i = 0; i < generator.lineCount - 1; ++i) {
                if (generator.lines[i + 1].startCharIdx > charPos)
                    return i;
            }

            return generator.lineCount - 1;
        }

        private void CreateCursorVertices() {
            m_CursorVertices = new UIVertex[4];

            for (int i = 0; i < m_CursorVertices.Length; i++) {
                m_CursorVertices[i] = UIVertex.simpleVert;
                m_CursorVertices[i].color = m_TextComponent.color;
                m_CursorVertices[i].uv0 = Vector2.zero;
            }
        }

        private float SumLineHeights(int endLine, TextGenerator generator) {
            float height = 0.0f;
            for (int i = 0; i < endLine; ++i) {
                height += generator.lines[i].height;
            }

            return height;
        }

        protected void UpdateLabel() {
            if (m_TextComponent != null && m_TextComponent.font != null && !m_PreventFontCallback) {
                // TextGenerator.Populate invokes a callback that's called for anything
                // that needs to be updated when the data for that font has changed.
                // This makes all Text components that use that font update their vertices.
                // In turn, this makes the InputField that's associated with that Text component
                // update its label by calling this UpdateLabel method.
                // This is a recursive call we want to prevent, since it makes the InputField
                // update based on font data that didn't yet finish executing, or alternatively
                // hang on infinite recursion, depending on whether the cached value is cached
                // before or after the calculation.
                //
                // This callback also occurs when assigning text to our Text component, i.e.,
                // m_TextComponent.text = processed;

                m_PreventFontCallback = true;

                string fullText;
                if (Input.compositionString.Length > 0)
                    fullText = text.Substring(0, m_CaretPosition) + Input.compositionString + text.Substring(m_CaretPosition);
                else
                    fullText = text;

                string processed = fullText;

                bool isEmpty = string.IsNullOrEmpty(fullText);

                if (m_Placeholder != null)
                    m_Placeholder.enabled = isEmpty;

                // If not currently editing the text, set the visible range to the whole text.
                // The UpdateLabel method will then truncate it to the part that fits inside the Text area.
                // We can't do this when text is being edited since it would discard the current scroll,
                // which is defined by means of the m_DrawStart and m_DrawEnd indices.
                if (!m_AllowInput) {
                    m_DrawStart = 0;
                    m_DrawEnd = m_Text.Length;
                }

                if (!isEmpty) {
                    // Determine what will actually fit into the given line
                    Vector2 extents = m_TextComponent.rectTransform.rect.size;

                    TextGenerationSettings settings = m_TextComponent.GetGenerationSettings(extents);
                    settings.generateOutOfBounds = true;

                    cachedInputTextGenerator.Populate(processed, settings);

                    SetDrawRangeToContainCaretPosition(caretSelectPositionInternal);

                    processed = processed.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, processed.Length) - m_DrawStart);

                    SetCaretVisible();
                }

                m_TextComponent.text = processed;
                MarkGeometryAsDirty();
                m_PreventFontCallback = false;
            }
        }

        private void SetDrawRangeToContainCaretPosition(int caretPos) {
            // the extents gets modified by the pixel density, so we need to use the generated extents since that will be in the same 'space' as
            // the values returned by the TextGenerator.lines[x].height for instance.
            Vector2 extents = cachedInputTextGenerator.rectExtents.size;
            if (multiLine) {
                var lines = cachedInputTextGenerator.lines;
                int caretLine = DetermineCharacterLine(caretPos, cachedInputTextGenerator);
                int height = (int) extents.y;

                // Have to compare with less or equal rather than just less.
                // The reason is that if the caret is between last char of one line and first char of next,
                // we want to interpret it as being on the next line.
                // This is also consistent with what DetermineCharacterLine returns.
                if (m_DrawEnd <= caretPos) {
                    // Caret comes after drawEnd, so we need to move drawEnd to a later line end that comes after caret.
                    m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, caretLine);
                    for (int i = caretLine; i >= 0 && i < lines.Count; --i) {
                        height -= lines[i].height;
                        if (height < 0)
                            break;

                        m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, i);
                    }
                }
                else {
                    if (m_DrawStart > caretPos) {
                        // Caret comes before drawStart, so we need to move drawStart to an earlier line start that comes before caret.
                        m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, caretLine);
                    }

                    int startLine = DetermineCharacterLine(m_DrawStart, cachedInputTextGenerator);
                    int endLine = startLine;
                    m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, endLine);
                    height -= lines[endLine].height;
                    while (true) {
                        if (endLine < lines.Count - 1) {
                            endLine++;
                            if (height < lines[endLine].height)
                                break;
                            m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, endLine);
                            height -= lines[endLine].height;
                        }
                        else if (startLine > 0) {
                            startLine--;
                            if (height < lines[startLine].height)
                                break;
                            m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, startLine);

                            height -= lines[startLine].height;
                        }
                        else
                            break;
                    }
                }
            }
            else {
                IList<UICharInfo> characters = cachedInputTextGenerator.characters;
                if (m_DrawEnd > cachedInputTextGenerator.characterCountVisible) {
                    m_DrawEnd = cachedInputTextGenerator.characterCountVisible;
                }

                float width = 0.0f;
                if (caretPos > m_DrawEnd || (caretPos == m_DrawEnd && m_DrawStart > 0)) {
                    // fit characters from the caretPos leftward
                    m_DrawEnd = caretPos;
                    for (m_DrawStart = m_DrawEnd - 1; m_DrawStart >= 0; --m_DrawStart) {
                        if (width + characters[m_DrawStart].charWidth > extents.x)
                            break;

                        width += characters[m_DrawStart].charWidth;
                    }

                    ++m_DrawStart; // move right one to the last character we could fit on the left
                }
                else {
                    if (caretPos < m_DrawStart) {
                        m_DrawStart = caretPos;
                    }

                    m_DrawEnd = m_DrawStart;
                }

                // fit characters rightward
                for (; m_DrawEnd < cachedInputTextGenerator.characterCountVisible; ++m_DrawEnd) {
                    width += characters[m_DrawEnd].charWidth;
                    if (width > extents.x) {
                        break;
                    }
                }
            }
        }

        protected void OnFocus() {
            SelectAll();
        }

        protected void SelectAll() {
            caretPositionInternal = text.Length;
            caretSelectPositionInternal = 0;
        }

        private void SetCaretVisible() {
            if (!m_AllowInput) return;

            m_CaretVisible = true;
            m_BlinkStartTime = Time.unscaledTime;
            SetCaretActive();
        }

        private void SetCaretActive() {
            if (!m_AllowInput)
                return;

            if (m_CaretBlinkRate > 0.0f) {
                if (m_BlinkCoroutine == null) {
                    m_BlinkCoroutine = StartCoroutine(CaretBlink());
                }
            }
            else {
                m_CaretVisible = true;
            }
        }

        IEnumerator CaretBlink() {
            // Always ensure caret is initially visible since it can otherwise be confusing for a moment.
            m_CaretVisible = true;
            yield return null;

            while (isFocused && m_CaretBlinkRate > 0) {
                // the blink rate is expressed as a frequency
                float blinkPeriod = 1f / m_CaretBlinkRate;

                // the caret should be ON if we are in the first half of the blink period
                bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2;
                if (m_CaretVisible != blinkState) {
                    m_CaretVisible = blinkState;
                    UpdateGeometry();
                }

                // Then wait again.
                yield return null;
            }

            m_BlinkCoroutine = null;
        }

        private static int GetLineStartPosition(TextGenerator gen, int line) {
            line = Mathf.Clamp(line, 0, gen.lines.Count - 1);
            return gen.lines[line].startCharIdx;
        }

        private static int GetLineEndPosition(TextGenerator gen, int line) {
            line = Mathf.Max(line, 0);
            if (line + 1 < gen.lines.Count) {
                return gen.lines[line + 1].startCharIdx;
            }

            return gen.characterCountVisible;
        }

    }

}