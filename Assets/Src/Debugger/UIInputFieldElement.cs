using Src.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Debugger {

    // <InputField type="Text" placeholder="" onValueChanged={} onFocus={} onBlur="{}" onMouseDown={} onKeyUp={}/>
    
    [AcceptFocus]
    public class UIInputFieldElement : UIElement {

        public string content;
        public string placeholder;
        public InputField.LineType lineType;
        public Color caretColor;
        public Color highlightColor;

        // validators?
        // formatters?
        
    }

}