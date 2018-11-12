using System.Collections.Generic;
using TMPro;
using UIForia.Animation;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Parsing.StyleParser {

    public class ParsedStyleSheet {

        public string id;
        public UIStyleGroup[] styles;
        public StyleAnimation[] animations;
        public Texture2D[] textures;
        public TMP_FontAsset fonts;
        public List<StyleVariable> variables;
        public List<string> dependencies;
        
        public UIStyleGroup GetStyleGroup(string uniqueStyleId) {
            if (styles == null) return null;
            for (int i = 0; i < styles.Length; i++) {
                if (styles[i].name == uniqueStyleId) {
                    return styles[i];
                }
            }

            return null;
        }

    }

}