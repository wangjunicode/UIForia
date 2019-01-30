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
            if (styles == null) return default;
            for (int i = 0; i < styles.Length; i++) {
                if (styles[i].name == uniqueStyleId) {
                    return styles[i];
                }
            }

            return default;
        }

        public bool GetStyleGroup(string uniqueStyleId, out UIStyleGroup group) {
            if (styles == null) {
                group = default;
                return false;
            }
            for (int i = 0; i < styles.Length; i++) {
                if (styles[i].name == uniqueStyleId) {
                    group = styles[i];
                    return true;
                }
            }

            group = default;
            return false;
        }

        
    }

}