using System;
using Src;
using Src.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Debugger {

    // <InputField type="Text" placeholder="" onValueChanged={} onFocus={} onBlur="{}" onMouseDown={} onKeyUp={}/>


    [AcceptFocus]
    [Template("Templates/InputField.xml")]
    public class UIInputFieldElement : UIElement {

        public delegate void ValueChanged<in T>(T value);
        
        public string content;
        public string placeholder;
        public InputField.LineType lineType;
        public Color caretColor;
        public Color highlightColor;
        public bool showPlaceholder = false;
        
        public event ValueChanged<string> onValueChanged;

        public UIInputFieldElement() {
            flags |= UIElementFlags.InputField;
        }
        
        public void SetText(string newText) {
            content = newText;
            onValueChanged?.Invoke(newText);
        } 
        
        // validators?
        // formatters?

//        public Action<string> onValueChanged;

    }

}