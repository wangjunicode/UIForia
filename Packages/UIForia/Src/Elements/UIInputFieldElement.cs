using System;
using UIForia.Attributes;
using UIForia.Bindings;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.UIInput;

using UnityEngine;

namespace UIForia.Elements {

    [Template(TemplateType.Internal, "Elements/UIInputFieldElement.xml")]
    public sealed class UIInputFieldElement : UIElement, IFocusable, IPropertyChangedHandler {

        public string text;
        public string placeholder;
        public bool selectAllOnFocus;
        public float caretBlinkRate = 0.85f;
        public event Action<string> onValueChanged;

        private UIContainerElement caret;
        private UIContainerElement highlight;
        private UITextElement textElement;
        private UISlotElement placeholderElement;
        private SelectionRange selectionRange = new SelectionRange(0, TextEdge.Right);
        private SelectionRange previousSelectionRange = new SelectionRange(0, TextEdge.Right);

        private float blinkStartTime;
        private bool hasFocus;
        private bool canSetCaret;

        public UIInputFieldElement() {
            flags |= UIElementFlags.BuiltIn;
        }

        protected static string clipboard {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        [OnPropertyChanged(nameof(text))]
        public void OnTextPropChanged(string field) {
            textElement?.SetText(text);
        }

        public string GetText() {
            return textElement.text;
        }

        public override void OnCreate() {
            text = text ?? string.Empty;
            caret = FindById<UIContainerElement>("cursor");
            highlight = FindById<UIContainerElement>("highlight");
            textElement = FindById<UITextElement>("text");
            placeholderElement = FindById<UISlotElement>("placeholder");

            caret.SetEnabled(false);
            highlight.SetEnabled(false);
            textElement.SetText(text);
        }

        public override void OnUpdate() {

            if (!hasFocus) return;

            if (selectionRange != previousSelectionRange) {
//                if (textElement.text.Length > 0) {
//                    if (selectionRange.HasSelection) {
//                        highlight.SetEnabled(true);
//                        highlight.MarkGeometryDirty();
//                    }
//                    else {
//                        highlight.SetEnabled(false);
//                    }
//                }

                blinkStartTime = Time.unscaledTime;
                previousSelectionRange = selectionRange;
            }

            float blinkPeriod = 1f / caretBlinkRate;
            bool blinkState = (Time.unscaledTime - blinkStartTime) % blinkPeriod < blinkPeriod / 2;
            if (canSetCaret) {
                caret.style.SetTransformPositionX(layoutResult.ContentRect.x + textElement.GetCursorPosition(selectionRange).x, StyleState.Normal);
                caret.style.SetTransformPositionY(textElement.layoutResult.localPosition.y, StyleState.Normal);
//            caret.style.SetTransformPositionX(layoutResult.contentRect.x + textElement.GetCursorPosition(selectionRange).x, StyleState.Normal);
//            caret.style.SetTransformPositionY(textElement.layoutResult.localPosition.y, StyleState.Normal);
            }

            if (blinkState) {
                caret.style.SetBackgroundColor(textElement.style.TextColor, StyleState.Normal);
            }
            else {
                caret.style.SetBackgroundColor(Color.clear, StyleState.Normal);                
            }
//            caret.SetEnabled(blinkState);
            canSetCaret = true;
        }

        [OnKeyDownWithFocus]
        private void EnterText(KeyboardInputEvent evt) {

            if (GetAttribute("disabled") != null) return;
            
            char c = evt.character;

            if (!textElement.style.TextFontAsset.characterDictionary.ContainsKey(c)) {
                return;
            }

            if (selectionRange.HasSelection) {
                selectionRange = textElement.DeleteRange(selectionRange);
                textElement.InsertText(selectionRange, c);
            }
            else {
                selectionRange = textElement.AppendText(c);
            }

            onValueChanged?.Invoke(textElement.text);
            placeholderElement.SetEnabled(string.IsNullOrEmpty(textElement.text));
            canSetCaret = false;
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

            evt.StopPropagation();

            Vector2 mouse = evt.MousePosition - layoutResult.screenPosition - layoutResult.ContentRect.position;

            if (evt.IsDoubleClick) {
                selectionRange = textElement.SelectWordAtPoint(mouse);
            }
            else if (evt.Shift) {
                selectionRange = textElement.SelectToPoint(selectionRange, mouse);
            }
            else {
                selectionRange = textElement.GetSelectionAtPoint(mouse);
            }
        }

        [OnKeyDownWithFocus(KeyCode.A, KeyboardModifiers.Control)]
        private void HandleSelectAll(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl) {
                SelectAll();
                evt.StopPropagation();
                canSetCaret = false;
            }
        }

        [OnKeyDownWithFocus(KeyCode.C, KeyboardModifiers.Control)]
        private void HandleCopy(KeyboardInputEvent evt) {
            if (evt.onlyControl && selectionRange.HasSelection) {
                clipboard = textElement.GetSubstring(selectionRange);
                evt.StopPropagation();
                canSetCaret = false;
            }
        }

        [OnKeyDownWithFocus(KeyCode.V, KeyboardModifiers.Control)]
        private void HandlePaste(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl) {
                textElement.AppendText(clipboard);
                placeholderElement.SetEnabled(string.IsNullOrEmpty(textElement.text));
                evt.StopPropagation();
                canSetCaret = false;
            }
        }

        [OnKeyDownWithFocus(KeyCode.X, KeyboardModifiers.Control)]
        private void HandleCut(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            if (evt.onlyControl && selectionRange.HasSelection) {
                clipboard = textElement.GetSubstring(selectionRange);
                DeleteSelection();
                placeholderElement.SetEnabled(string.IsNullOrEmpty(textElement.text));
                canSetCaret = false;
            }
        }

        [OnKeyDownWithFocus(KeyCode.Backspace)]
        private void HandleBackspace(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            selectionRange = textElement.DeleteTextBackwards(selectionRange);
            placeholderElement.SetEnabled(string.IsNullOrEmpty(textElement.text));
            canSetCaret = false;
        }

        [OnKeyDownWithFocus(KeyCode.Delete)]
        private void HandleDelete(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            selectionRange = textElement.DeleteTextForwards(selectionRange);
            placeholderElement.SetEnabled(string.IsNullOrEmpty(textElement.text));
            canSetCaret = false;
        }

        [OnKeyDownWithFocus(KeyCode.LeftArrow)]
        private void HandleLeftArrow(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            selectionRange = textElement.MoveCursorLeft(selectionRange, evt.shift);
            canSetCaret = false;
        }

        [OnKeyDownWithFocus(KeyCode.RightArrow)]
        private void HandleRightArrow(KeyboardInputEvent evt) {
            if (GetAttribute("disabled") != null) return;
            selectionRange = textElement.MoveCursorRight(selectionRange, evt.shift);
            canSetCaret = false;
        }

        private void SelectAll() {
            if (GetAttribute("disabled") != null) return;
            selectionRange = textElement.SelectAll();
            canSetCaret = false;
        }

        private void DeleteSelection() {
        }

        [OnDragCreate]
        public TextSelectDragEvent CreateDragEvent(MouseInputEvent evt) {
            TextSelectDragEvent retn = new TextSelectDragEvent(this);
            retn.onUpdate += HandleDragUpdate;
            Vector2 mouse = evt.MousePosition - layoutResult.screenPosition - layoutResult.ContentRect.position;
            selectionRange = textElement.BeginSelection(mouse);
            return retn;
        }

        private void HandleDragUpdate(DragEvent obj) {
            Vector2 mouse = obj.MousePosition - layoutResult.screenPosition - layoutResult.ContentRect.position;
            selectionRange = textElement.SelectToPoint(selectionRange, mouse);
        }

        public void Focus() {
            if (GetAttribute("disabled") != null) return;
            hasFocus = true;
            blinkStartTime = Time.unscaledTime;
            caret.SetEnabled(true);
        }

        public void Blur() {
            hasFocus = false;
            caret.SetEnabled(false);
            highlight.SetEnabled(false);

        }

        public void OnPropertyChanged(string propertyName, object oldValue) {
            if (textElement == null) return;

            if (propertyName == nameof(text)) {
                placeholderElement.SetEnabled(string.IsNullOrEmpty(text));
                textElement.SetText(text);
                selectionRange = textElement.ValidateSelectionRange(selectionRange);
            }
        }

        public override string GetDisplayName() {
            return "TextInput";
        }

        public class TextSelectDragEvent : CallbackDragEvent {

            public TextSelectDragEvent(UIElement origin) : base(origin) {
            }

        }
    }
}
