using System;
using JetBrains.Annotations;
using UIForia.Attributes;
using UIForia.Graphics;
using UIForia.Rendering;
using UIForia.UIInput;
using Unity.Mathematics;
using UnityEngine;

#pragma warning disable 0649
namespace UIForia.Elements {

    // todo use StructList<char> instead of string to alloc less
    public abstract class UIInputElement : BaseInputElement, IFocusableEvented {

        internal string text;


        private string m_placeholder;

        public string placeholder {
            get { return string.IsNullOrEmpty(m_placeholder) ? "" : m_placeholder; }
            set { m_placeholder = value; }
        }

        public event Action<FocusEvent> onFocus;
        public event Action<BlurEvent> onBlur;

        public bool autofocus;

        protected float holdDebounce = 0.05f;
        protected float timestamp;

        public float caretBlinkRate = 0.85f;
        protected float blinkStartTime;

        // protected SelectionRange selectionRange;

        protected bool hasFocus;
        protected bool selectAllOnFocus;
        protected bool isSelecting;

        protected Vector2 textScroll = new Vector2(0, 0);

        protected float keyLockTimestamp;

        private bool isReady;

        public UIInputElement() {
            //   selectionRange = new SelectionRange(0);
        }

        protected static string clipboard {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        public override void OnCreate() {
            text = text ?? string.Empty;
            style.SetPainter("UIForia::Input", StyleState.Normal);
            application.InputSystem.RegisterFocusable(this);
        }

        public override void OnEnable() {
            base.OnEnable();
            EmitTextChanged();
            if (autofocus) {
                application.InputSystem.RequestFocus(this);
            }

            if (string.IsNullOrEmpty(m_placeholder)) {
                FindById("placeholder-text")?.SetAttribute("empty", "true");
            }
        }

        public override void OnUpdate() {
            if (isReady) {
                ScrollToCursor();
            }

            isReady = true;
        }

        protected void EmitTextChanged() {
            textElement.SetText(text);
        }

        protected abstract void HandleSubmit();
        protected abstract void HandleCharactersDeletedForwards();
        protected abstract void HandleCharactersDeletedBackwards();
        protected abstract void HandleCharactersEntered(string characters);

        public override void OnDestroy() {
            Blur();
            application.InputSystem.UnRegisterFocusable(this);
        }

        public override void OnDisable() {
            Blur();
        }

        public override string GetDisplayName() {
            return "InputElement";
        }

        [UsedImplicitly]
        [OnMouseClick]
        public void OnMouseClick(MouseInputEvent evt) {
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
                textElement.SelectWordAtPoint(mouse);
            }
            else if (evt.IsTripleClick) {
                textElement.SelectLineAtPoint(mouse);
            }
            else {
                SelectionCursor caretCursor = textElement.GetSelectionCursorAtPoint(mouse);
                textElement.SetSelection(caretCursor, SelectionCursor.Invalid);
                ScrollToCursor();
            }

            evt.StopPropagation();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus]
        [OnKeyHeldDownWithFocus]
        public void EnterText(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (HasDisabledAttr()) return;

            switch (evt.keyCode) {
                case KeyCode.Home:
                    HandleHome(evt);
                    break;
                case KeyCode.End:
                    HandleEnd(evt);
                    break;
                case KeyCode.Backspace:
                    HandleBackspace(evt);
                    break;
                case KeyCode.Delete:
                    HandleDelete(evt);
                    break;
                case KeyCode.LeftArrow:
                    HandleLeftArrow(evt);
                    break;
                case KeyCode.RightArrow:
                    HandleRightArrow(evt);
                    break;
                case KeyCode.C when evt.onlyControl && textElement.HasSelection:
                    HandleCopy(evt);
                    break;
                case KeyCode.V when evt.onlyControl:
                    HandlePaste(evt);
                    break;
                case KeyCode.X when evt.onlyControl && textElement.HasSelection:
                    HandleCut(evt);
                    break;
                case KeyCode.A when evt.onlyControl:
                    HandleSelectAll(evt);
                    break;

                default:
                    OnTextEntered(evt);
                    break;
            }
        }

        private void OnTextEntered(KeyboardInputEvent evt) {
            if (evt.ctrl) {
                return;
            }

            char c = evt.character;

            if (evt.keyCode == KeyCode.Return) {
                if (!InitKeyPress(evt)) {
                    return;
                }

                HandleSubmit();
                return;
            }

            if (evt.keyCode == KeyCode.Tab) {
                return;
            }

            if (c == '\n' || c == '\t') return;

            if (!textElement.FontHasCharacter(c)) {
                return;
            }

            HandleCharactersEntered(c.ToString());
            ScrollToCursor();
        }

        protected unsafe void ScrollToCursor() {
            if (!hasFocus) {
                return;
            }

            Rect rect = VisibleTextRect;

            Vector2 cursor = textElement.GetCursorRect().position;

            int lineIndex = textElement.textInfo->FindNearestLine(cursor);

            if (lineIndex < 0) {
                textScroll.x = 0;
                return;
            }

            float lineWidth = textElement.textInfo->lineInfoList.array[lineIndex].width;

            if (cursor.x - textScroll.x >= rect.width) {
                textScroll.x = (cursor.x - rect.width + rect.x);
            }
            else if (cursor.x - textScroll.x < rect.xMin) {
                textScroll.x = (cursor.x - rect.x);
                if (textScroll.x < 0) textScroll.x = 0;
            }

            if (rect.width >= lineWidth) {
                textScroll.x = 0;
            }
        }

        private void HandleHome(KeyboardInputEvent evt) {
            SelectionCursor cursor = textElement.GetSelectionStartCursor();
            textElement.MoveToStartOfLine(evt.shift);
            //selectionRange = textElement.MoveToStartOfLine(selectionRange, evt.shift);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        private void HandleEnd(KeyboardInputEvent evt) {
            // selectionRange = textElement.MoveToEndOfLine(selectionRange, evt.shift);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        private void HandleBackspace(KeyboardInputEvent evt) {
            if (!InitKeyPress(evt)) return;

            HandleCharactersDeletedBackwards();
            ScrollToCursor();
        }

        private bool InitKeyPress(in KeyboardInputEvent evt) {
            if (evt.eventType == InputEventType.KeyHeldDown) {
                if (!CanTriggerHeldKey()) return false;
                timestamp = Time.unscaledTime;
            }
            else {
                keyLockTimestamp = Time.unscaledTime;
            }

            blinkStartTime = Time.unscaledTime;
            return true;
        }

        private void HandleDelete(KeyboardInputEvent evt) {
            if (!InitKeyPress(evt)) return;
            if (evt.ctrl || evt.command) {
                textElement.DeleteForwards();
            }

            HandleCharactersDeletedForwards();
            ScrollToCursor();
        }

        private void HandleLeftArrow(KeyboardInputEvent evt) {
            if (!InitKeyPress(evt)) return;
            textElement.MoveCursorLeft(evt.shift, evt.command || evt.ctrl);
            ScrollToCursor();
        }

        private void HandleRightArrow(KeyboardInputEvent evt) {
            if (!InitKeyPress(evt)) return;
            textElement.MoveCursorRight(evt.shift, evt.command || evt.ctrl);
            ScrollToCursor();
        }

        private void HandleCopy(KeyboardInputEvent evt) {
            if (evt.onlyControl && textElement.HasSelection) {
                clipboard = textElement.GetSelectedString();
                evt.StopPropagation();
            }
        }

        private void HandleCut(KeyboardInputEvent evt) {
            clipboard = textElement.GetSelectedString();
            HandleCharactersDeletedBackwards();
            evt.StopPropagation();
        }

        private void HandlePaste(KeyboardInputEvent evt) {
            HandleCharactersEntered(clipboard);
            evt.StopPropagation();
        }

        private void HandleSelectAll(KeyboardInputEvent evt) {
            textElement.SelectAll();
            evt.StopPropagation();
        }

        public bool HasDisabledAttr() {
            return GetAttribute("disabled") != null;
        }

        public bool CanTriggerHeldKey() {
            if (GetAttribute("disabled") != null) return false;
            if (Time.unscaledTime - keyLockTimestamp < 0.5f) {
                return false;
            }

            if (Time.unscaledTime - timestamp < holdDebounce) {
                return false;
            }

            return true;
        }

        [UsedImplicitly]
        [OnDragCreate]
        public TextSelectDragEvent CreateDragEvent(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            if (!hasFocus) {
                application.InputSystem.RequestFocus(this);
            }

            Vector2 mouseDownPosition = evt.LeftMouseDownPosition - layoutResult.screenPosition - layoutResult.ContentRect.position + textScroll;
            TextSelectDragEvent retn = new TextSelectDragEvent(this, textElement.GetSelectionCursorAtPoint(mouseDownPosition));
            Vector2 mousePosition = evt.MousePosition - layoutResult.screenPosition - layoutResult.ContentRect.position + textScroll;
            int indexAtDownPoint = textElement.GetIndexAtPoint(mouseDownPosition);
            int indexAtPoint = textElement.GetIndexAtPoint(mousePosition);
            if (indexAtDownPoint < indexAtPoint) {
                // selectionRange = new SelectionRange(indexAtPoint, indexAtDownPoint);
            }

            else {
                //selectionRange = new SelectionRange(indexAtDownPoint, indexAtPoint);
            }

            return retn;
        }

        protected Rect VisibleTextRect {
            get { return layoutResult.ContentRect; }
        }

        public bool Focus() {
            if (GetAttribute("disabled") != null) {
                return false;
            }

            ScrollToCursor();
            hasFocus = true;
            onFocus?.Invoke(new FocusEvent());
            return true;
        }

        public void Blur() {
            hasFocus = false;
            // selectionRange = new SelectionRange(selectionRange.cursorIndex);
            onBlur?.Invoke(new BlurEvent());
        }

        public class TextSelectDragEvent : DragEvent {

            protected readonly UIInputElement _uiInputElement;

            protected readonly SelectionCursor originCursor;

            public TextSelectDragEvent(UIInputElement origin, SelectionCursor originCursor) {
                this._uiInputElement = origin;
                _uiInputElement.isSelecting = true;
                this.originCursor = originCursor;
            }

            public override void Update() {
                Vector2 mouse = _uiInputElement.RelativePoint(MousePosition);
                mouse += _uiInputElement.textScroll;

                SelectionCursor caretCursor = _uiInputElement.textElement.GetSelectionCursorAtPoint(mouse);
                _uiInputElement.textElement.SetSelection(caretCursor, originCursor);
                _uiInputElement.ScrollToCursor();
            }

            public override void OnComplete() {
                _uiInputElement.isSelecting = false;
                // _uiInputElement.selectionRange = new SelectionRange(_uiInputElement.selectionRange.selectIndex, _uiInputElement.selectionRange.cursorIndex);
            }

        }

        private Rect GetCaretRect() {
            Rect rect = textElement.GetCursorRect();
            rect.x += layoutResult.HorizontalPaddingBorderStart;
            rect.y += layoutResult.VerticalPaddingBorderStart;
            rect.x -= textScroll.x;
            rect.y -= textScroll.y;
            return rect;
        }

        [CustomPainter("UIForia::Input")]
        public class InputElementPainter : StandardRenderBox2 {

            public override void PaintBackground3(RenderContext3 ctx) {
                base.PaintBackground3(ctx);

                if (!(element is UIInputElement inputElement)) return;

                float4x4 translate = float4x4.Translate(new float3(-inputElement.textScroll.x, -inputElement.textScroll.y, 0));
                ctx.SetMatrix(math.mul(translate, inputElement.textElement.layoutResult.GetWorldMatrix()));
                ctx.PaintElementBackground(inputElement.textElement);
            }

            public override void PaintForeground3(RenderContext3 ctx) {
                base.PaintForeground3(ctx);
                if (!(element is UIInputElement inputElement)) return;

                float blinkPeriod = 1f / inputElement.caretBlinkRate;

                bool blinkState = (Time.unscaledTime - inputElement.blinkStartTime) % blinkPeriod < blinkPeriod / 2;


                // if (!inputElement.isSelecting && inputElement.hasFocus && blinkState) {
                //   if (blinkState) {
                Rect rect = inputElement.GetCaretRect();
                ctx.DrawElement((int) rect.x, rect.y, new ElementDrawDesc((int) rect.width, rect.height) {
                    backgroundColor = Color.black
                });
                //    }

                // base.PaintBackground(ctx);
                //
                // UIInputElement inputElement = (UIInputElement) element;
                //
                // path.Clear();
                // path.SetTransform(inputElement.layoutResult.matrix.ToMatrix4x4());
                //
                // float blinkPeriod = 1f / inputElement.caretBlinkRate;
                //
                // bool blinkState = (Time.unscaledTime - inputElement.blinkStartTime) % blinkPeriod < blinkPeriod / 2;
                //
                // Rect contentRect = inputElement.layoutResult.ContentRect;
                //
                // TextInfoOld textInfoOld = null; //inputElement.textElement.textInfo;
                //
                // // float baseLineHeight = textInfo.rootSpan.textStyle.fontAsset.faceInfo.LineHeight;
                // // float scaledSize = textInfo.rootSpan.fontSize / textInfo.rootSpan.textStyle.fontAsset.faceInfo.PointSize;
                // // float lh = baseLineHeight * scaledSize;
                //
                // if (!inputElement.isSelecting && inputElement.hasFocus && blinkState) {
                //     path.BeginPath();
                //     path.SetStroke(inputElement.style.CaretColor);
                //     path.SetStrokeWidth(1f);
                //     // float2 p = inputElement.textElement.GetCursorPosition(inputElement.selectionRange.cursorIndex) - inputElement.textScroll;
                //     // path.MoveTo(inputElement.layoutResult.ContentRect.min + p);
                //     // path.VerticalLineTo(inputElement.layoutResult.ContentRect.yMax);
                //     // path.EndPath();
                //     // path.Stroke();
                // }
                //
                // if (inputElement.selectionRange.HasSelection) {
                //     RangeInt lineRange = new RangeInt(0, 1); //textInfo.GetLineRange(selectionRange));textInfo.GetLineRange(selectionRange);
                //     path.BeginPath();
                //     path.SetFill(inputElement.style.SelectionBackgroundColor);
                //
                //     if (lineRange.length > 1) {
                //         // todo this doesn't really work yet
                //         for (int i = lineRange.start + 1; i < lineRange.end - 1; i++) {
                //             //                        Rect rect = textInfo.GetLineRect(i);
                //             //                        rect.x += contentRect.x;
                //             //                        rect.y += contentRect.y;
                //             //                        path.Rect(rect);
                //         }
                //     }
                //     else {
                //         // Rect rect = inputElement.textElement.GetLineRect(lineRange.start);
                //         // float2 cursorPosition = inputElement.textElement.GetCursorPosition(inputElement.selectionRange.cursorIndex) - inputElement.textScroll;
                //         // Vector2 selectPosition = inputElement.textElement.GetSelectionPosition(inputElement.selectionRange) - inputElement.textScroll;
                //         // float minX = Mathf.Min(cursorPosition.x, selectPosition.x);
                //         // float maxX = Mathf.Max(cursorPosition.x, selectPosition.x);
                //         // minX += contentRect.x;
                //         // maxX += contentRect.x;
                //         // rect.y += contentRect.y;
                //         // float x = Mathf.Max(minX, contentRect.x);
                //         // float cursorToContentEnd = contentRect.width;
                //         // float cursorToMax = maxX - x;
                //         // path.Rect(x, rect.y, Mathf.Min(cursorToContentEnd, cursorToMax), rect.height);
                //     }
                //
                //     path.Fill();
                // }
                //
                // ctx.DrawPath(path);
            }

        }

    }

}