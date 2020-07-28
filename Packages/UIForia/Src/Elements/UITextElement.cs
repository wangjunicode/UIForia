using System;
using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UIForia.Util;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Elements {

    [TemplateTagName("Text")]
    public class UITextElement : UIElement, IStyleChangeHandler {

        internal string text;
        internal ITextProcessor _processor;
        internal int textInfoId;
        internal int lastUpdateFrame;

        internal IList<TextEffect> __textEffects;
        
        // setting this and text in same frame will run update twice, improve this
        public ITextProcessor processor {
            get => _processor;
            set {
                if (_processor != value) {
                    _processor = value;
                    if (textInfoId != 0) {
                        application.textSystem.UpdateText(this);
                    }
                }
            }
        }

        public string GetText() {
            return text;
        }

        public void SetTextFromCharacters(char[] newText, int length) {

            if (text == null || text.Length != length) {
                // needs update
                text = new string(newText, 0, length);
                application.textSystem.UpdateText(this);
                return;
            }

            unsafe {

                fixed (char* oldTextPtr = text)
                fixed (char* newTextPtr = newText) {

                    if (UnsafeUtility.MemCmp(oldTextPtr, newTextPtr, 2 * length) != 0) {
                        text = new string(newText, 0, length);
                        application.textSystem.UpdateText(this);
                    }

                }

            }

        }

        public void SetText(string newText) {

            if (newText == null) newText = string.Empty;
            
            if (text == null || text.Length != newText.Length) {
                // needs update
                text = newText;
                application.textSystem.UpdateText(this);
                return;
            }

            unsafe {

                fixed (char* oldTextPtr = text)
                fixed (char* newTextPtr = newText) {

                    if (UnsafeUtility.MemCmp(oldTextPtr, newTextPtr, 2 * newText.Length) != 0) {
                        text = newText;
                        application.textSystem.UpdateText(this);
                    }

                }

            }

        }

        public override string GetDisplayName() {
            return "Text";
        }

        public void OnStylePropertyChanged(in StyleProperty property) {
            // switch (property.propertyId) {
            //
            //     case StylePropertyId.TextFontSize:
            //         textSpan.SetFontSize(style.GetResolvedFontSize());
            //         break;
            //
            //     case StylePropertyId.TextFontStyle:
            //         textSpan.SetFontStyle(property.AsFontStyle);
            //         break;
            //
            //     case StylePropertyId.TextAlignment:
            //         textSpan.SetTextAlignment(property.AsTextAlignment);
            //         break;
            //
            //     case StylePropertyId.TextFontAsset:
            //         textSpan.SetFont(property.AsFont);
            //         break;
            //
            //     case StylePropertyId.TextTransform:
            //         textSpan.SetTextTransform(property.AsTextTransform);
            //         break;
            //
            //     case StylePropertyId.TextWhitespaceMode:
            //         textSpan.SetWhitespaceMode(property.AsWhitespaceMode);
            //         break;
            //
            //     case StylePropertyId.TextColor:
            //         textSpan.SetTextColor(property.AsColor);
            //         break;
            //
            //     case StylePropertyId.TextGlowColor:
            //         textSpan.SetGlowColor(property.AsColor);
            //         break;
            //
            //     case StylePropertyId.TextGlowOffset:
            //         textSpan.SetGlowOffset(property.AsFloat);
            //         break;
            //
            //     case StylePropertyId.TextGlowOuter:
            //         textSpan.SetGlowOuter(property.AsFloat);
            //         break;
            //
            //     case StylePropertyId.TextUnderlayX:
            //         textSpan.SetUnderlayX(property.AsFloat);
            //         break;
            //
            //     case StylePropertyId.TextUnderlayY:
            //         break;
            //
            //     case StylePropertyId.TextUnderlayDilate:
            //         textSpan.SetUnderlayDilate(property.AsFloat);
            //         break;
            //
            //     case StylePropertyId.TextUnderlayColor:
            //         textSpan.SetUnderlayColor(property.AsColor);
            //         break;
            //
            //     case StylePropertyId.TextUnderlaySoftness:
            //         textSpan.SetUnderlaySoftness(property.AsFloat);
            //         break;
            //
            //     case StylePropertyId.TextFaceDilate:
            //         textSpan.SetFaceDilate(property.AsFloat);
            //         break;
            //
            //     case StylePropertyId.TextGlowPower:
            //     case StylePropertyId.TextUnderlayType:
            //         break;
            // }
        }

        public int GetIndexAtPoint(Vector2 mouse) {
            throw new NotImplementedException();
        }

        public SelectionRange SelectWordAtPoint(Vector2 mouse) {
            throw new NotImplementedException();
        }

        public SelectionRange SelectLineAtPoint(Vector2 mouse) {
            throw new NotImplementedException();
        }

        public bool HasCharacter(char c) {
            throw new NotImplementedException();
        }

        public void Layout(Vector2 zero, float maxValue) {
            throw new NotImplementedException();
        }

        public Vector2 GetCursorPosition(int selectionRangeCursorIndex) {
            throw new NotImplementedException();
        }

        public SelectionRange MoveToStartOfLine(SelectionRange selectionRange, bool evtShift) {
            throw new NotImplementedException();
        }

        public SelectionRange MoveToEndOfLine(SelectionRange selectionRange, bool evtShift) {
            throw new NotImplementedException();
        }

        public SelectionRange MoveCursorLeft(SelectionRange selectionRange, bool evtShift, bool evtCommand) {
            throw new NotImplementedException();
        }

        public SelectionRange MoveCursorRight(SelectionRange selectionRange, bool evtShift, bool evtCommand) {
            throw new NotImplementedException();
        }

        public string GetSelectedString(SelectionRange selectionRange) {
            throw new NotImplementedException();
        }

        public Rect GetLineRect(int lineRangeStart) {
            throw new NotImplementedException();
        }

        public Vector2 GetSelectionPosition(SelectionRange inputElementSelectionRange) {
            throw new NotImplementedException();
        }

        internal bool TryGetTextInfo(out TextInfo textInfo) {
            if (textInfoId != 0) {
                textInfo = application.textSystem.textInfoMap[textInfoId];
                return true;
            }

            textInfo = default;
            return false;
        }

    }

}