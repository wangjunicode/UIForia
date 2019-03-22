using System;
using SVGX;
using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UIForia.UIInput;
using UnityEngine;

namespace UIForia.Elements {

    public class InputElementText : UITextElement { }

    [Template(TemplateType.Internal, "Elements/InputElement.xml")]
    public class InputElement : UIElement, IFocusable, ISVGXPaintable {

        internal TextInfo2 textInfo;
        internal string text;
        internal string placeholder;
        
        private float holdDebounce = 0.05f;
        private float timestamp;
        private string value;

        public Color caretColor = Color.black;
        public float caretBlinkRate = 0.85f;
        private float blinkStartTime;

        private SelectionRange selectionRange;

        private bool hasFocus;
        private bool selectAllOnFocus;
        private bool isSelecting;

        private Vector2 textScroll = new Vector2(0, 0);

        public event Action<string> onValueChanged;

        public InputElement() {
            flags |= UIElementFlags.BuiltIn;
            selectionRange = new SelectionRange(0, TextEdge.Left);
        }

        protected static string clipboard {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        public override void OnCreate() {
            this.text = "overflow me please thats great"; //"This is kinda overflow even more";
            style.SetPainter("self", StyleState.Normal);
            textInfo = FindFirstByType<UITextElement>().textInfo;
            textInfo.UpdateSpan(0, text);
        }

        public override string GetDisplayName() {
            return "InputElement";
        }

        private void HandleTextChanged() {
            if (textInfo.CharCount == 0) { }

            onValueChanged?.Invoke(textInfo.GetAllText());
        }

        [OnMouseDown]
        private void OnMouseDown(MouseInputEvent evt) {
            bool hadFocus = hasFocus;

            if (evt.IsConsumed || (!hasFocus && !Input.RequestFocus(this))) {
                return;
            }

            if (!hadFocus && selectAllOnFocus) {
                return;
            }

            blinkStartTime = Time.unscaledTime;
            Vector2 mouse = evt.MousePosition - layoutResult.screenPosition - layoutResult.ContentRect.position;
            mouse += textScroll;

            if (evt.IsDoubleClick) {
                selectionRange = textInfo.SelectWordAtPoint(mouse);
            }
            else if (evt.IsTripleClick) {
                selectionRange = textInfo.SelectLineAtPoint(mouse);
            }
            else {
                selectionRange = textInfo.GetSelectionAtPoint(mouse);
                ScrollToCursor();
            }

            evt.StopPropagation();
        }

        [OnKeyDownWithFocus]
        private void EnterText(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;

            char c = evt.character;

            if (!textInfo.spanList[0].textStyle.font.characterDictionary.ContainsKey(c)) {
                return;
            }

            selectionRange = textInfo.InsertText(selectionRange, c);
            textInfo.Layout();
            ScrollToCursor();
            HandleTextChanged();
            evt.StopPropagation();
        }

        private void ScrollToCursor() {
            Rect rect = VisibleTextRect;
            Vector2 cursor = textInfo.GetCursorPosition(selectionRange);
            if (cursor.x - textScroll.x >= rect.xMax) {
                textScroll.x = (cursor.x - VisibleTextRect.xMax + VisibleTextRect.x);
            }
            else if (cursor.x - textScroll.x < rect.xMin) {
                textScroll.x = (cursor.x - VisibleTextRect.x);
                if (textScroll.x < 0) textScroll.x = 0;
            }
        }

        [OnKeyDownWithFocus(KeyCode.Home)]
        private void HandleHome(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            selectionRange = textInfo.MoveToStartOfLine(selectionRange);
            blinkStartTime = Time.unscaledTime;
            evt.StopPropagation();
        }

        [OnKeyDownWithFocus(KeyCode.End)]
        private void HandleEnd(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            selectionRange = textInfo.MoveToEndOfLine(selectionRange);
            blinkStartTime = Time.unscaledTime;
            evt.StopPropagation();
        }

        [OnKeyDownWithFocus(KeyCode.Backspace)]
        private void HandleBackspace(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            selectionRange = textInfo.DeleteTextBackwards(selectionRange);
            blinkStartTime = Time.unscaledTime;
            HandleTextChanged();
            evt.StopPropagation();
        }

        [OnKeyDownWithFocus(KeyCode.Delete)]
        private void HandleDelete(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            selectionRange = textInfo.DeleteTextForwards(selectionRange);
            blinkStartTime = Time.unscaledTime;
            HandleTextChanged();
            evt.StopPropagation();
        }

        [OnKeyHeldWithFocus(KeyCode.LeftArrow)]
        private void HandleLeftArrow(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (Time.unscaledTime - timestamp < holdDebounce) {
                return;
            }

            timestamp = Time.unscaledTime;
            selectionRange = textInfo.MoveCursorLeft(selectionRange, evt.shift);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
            evt.StopPropagation();
        }

        [OnKeyHeldWithFocus(KeyCode.RightArrow)]
        private void HandleRightArrow(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (Time.unscaledTime - timestamp < holdDebounce) {
                return;
            }
            timestamp = Time.unscaledTime;
            selectionRange = textInfo.MoveCursorRight(selectionRange, evt.shift);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
            evt.StopPropagation();
        }

        [OnKeyDownWithFocus(KeyCode.C, KeyboardModifiers.Control)]
        private void HandleCopy(KeyboardInputEvent evt) {
            if (evt.onlyControl && selectionRange.HasSelection) {
                clipboard = textInfo.GetSelectedString(selectionRange);
                evt.StopPropagation();
            }
        }

        [OnKeyDownWithFocus(KeyCode.X, KeyboardModifiers.Control)]
        private void HandleCut(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl && selectionRange.HasSelection) {
                clipboard = textInfo.GetSelectedString(selectionRange);
                selectionRange = textInfo.DeleteTextBackwards(selectionRange);
                HandleTextChanged();
                evt.StopPropagation();
            }
        }

        [OnKeyDownWithFocus(KeyCode.V, KeyboardModifiers.Control)]
        private void HandlePaste(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl) {
                textInfo.InsertText(selectionRange, clipboard);
                evt.StopPropagation();
                HandleTextChanged();
            }
        }

        [OnKeyDownWithFocus(KeyCode.A, KeyboardModifiers.Control)]
        private void HandleSelectAll(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl) {
                selectionRange = textInfo.SelectAll();
                evt.StopPropagation();
            }
        }

        [OnDragCreate]
        private TextSelectDragEvent CreateDragEvent(MouseInputEvent evt) {
            TextSelectDragEvent retn = new TextSelectDragEvent(this);
            Vector2 mouse = evt.MouseDownPosition - layoutResult.screenPosition - layoutResult.ContentRect.position;
            selectionRange = textInfo.GetSelectionAtPoint(mouse);
            return retn;
        }

        public void Paint(ImmediateRenderContext ctx, in SVGXMatrix matrix) {
            ctx.SetTransform(matrix);
            SVGXRenderSystem.PaintElement(ctx, this);

            float blinkPeriod = 1f / caretBlinkRate;

            bool blinkState = (Time.unscaledTime - blinkStartTime) % blinkPeriod < blinkPeriod / 2;

            Rect contentRect = layoutResult.ContentRect;
//           ctx.EnableScissorRect(VisibleTextRect);
            ctx.DisableScissorRect();
            if (isSelecting) {
                ctx.BeginPath();
                ctx.SetStroke(caretColor);
                ctx.SetStrokeWidth(1f);
                Vector2 p = textInfo.GetSelectionPosition(selectionRange) - textScroll;
                ctx.MoveTo(layoutResult.ContentRect.min + p + new Vector2(0, 4f)); // todo remove + 4 on y
                ctx.VerticalLineTo(layoutResult.ContentRect.y + p.y + style.TextFontSize);
                ctx.Stroke();
            }

            if (!isSelecting && hasFocus && blinkState) {
                ctx.BeginPath();
                ctx.SetStroke(caretColor);
                ctx.SetStrokeWidth(1f);
                Vector2 p = textInfo.GetCursorPosition(selectionRange) - textScroll;
                ctx.MoveTo(layoutResult.ContentRect.min + p + new Vector2(0, 4f)); // todo remove + 4 on y
                ctx.VerticalLineTo(layoutResult.ContentRect.y + p.y + style.TextFontSize);
                ctx.Stroke();
            }

            if (selectionRange.HasSelection) {
                RangeInt lineRange = new RangeInt(0, 1); //textInfo.GetLineRange(selectionRange));textInfo.GetLineRange(selectionRange);
                ctx.BeginPath();
                ctx.SetFill(new Color(0.5f, 0, 0, 0.5f));

                if (lineRange.length > 1) {
                    // todo this doesn't really work yet
                    for (int i = lineRange.start + 1; i < lineRange.end - 1; i++) {
                        Rect rect = textInfo.GetLineRect(i);
                        rect.x += contentRect.x;
                        rect.y += contentRect.y;
                        ctx.Rect(rect);
                    }
                }
                else {
                    // todo the highlight is wrong when scrolled
                    Rect rect = textInfo.GetLineRect(lineRange.start);
                    Vector2 cursorPosition = textInfo.GetCursorPosition(selectionRange) - textScroll;
                    Vector2 selectPosition = textInfo.GetSelectionPosition(selectionRange) - textScroll;
                    float minX = Mathf.Min(cursorPosition.x, selectPosition.x);
                    float maxX = Mathf.Max(cursorPosition.x, selectPosition.x);
                    minX += contentRect.x;
                    maxX += contentRect.x;
                    rect.y += contentRect.y;
                    ctx.Rect(minX, rect.y, maxX - minX, rect.height);
                }

                ctx.Fill();
            }

            ctx.BeginPath();
            ctx.Rect(VisibleTextRect);
            ctx.SetFill(new Color32(0, 255, 0, 125));
            ctx.Fill();
            
            ctx.BeginPath();
            ctx.SetFill(style.TextColor);
            ctx.Text(contentRect.x - textScroll.x, contentRect.y, textInfo);
            ctx.Fill();
        }

        private Rect VisibleTextRect {
            get {
                Rect contentRect = layoutResult.ContentRect;
                return new Rect(
                    layoutResult.screenPosition.x - contentRect.x,
                    layoutResult.screenPosition.y - contentRect.y,
                    contentRect.width,
                    contentRect.height
                );
            }
        }

        public void Focus() {
            if (GetAttribute("disabled") != null) {
                return;
            }

            hasFocus = true;
        }

        public void Blur() {
            hasFocus = false;
        }

        private class TextSelectDragEvent : DragEvent {

            private readonly InputElement inputElement;

            public TextSelectDragEvent(InputElement origin) : base(origin) {
                this.inputElement = origin;
                inputElement.isSelecting = true;
            }

            public override void Update() {
                Vector2 mouse = MousePosition - inputElement.layoutResult.screenPosition - inputElement.layoutResult.ContentRect.position;
                mouse += inputElement.textScroll;
                inputElement.selectionRange = inputElement.textInfo.SelectToPoint(inputElement.selectionRange, mouse);
            }

            public override void OnComplete() {
                inputElement.isSelecting = false;
                inputElement.selectionRange = inputElement.selectionRange.Invert();
            }

        }

    }

}