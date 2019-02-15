using System.Collections.Generic;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEditor;

namespace UIForia.Compilers.Style {
    
    public class StyleCompileContext {
        
        private static readonly LightList<string> s_CurrentlyResolvingReferences = LightListPool<string>.Get();
        private static readonly LightList<string> s_CurrentlyImportingStyleSheets = LightListPool<string>.Get();

        public Dictionary<string, ConstNode> constNodes = new Dictionary<string, ConstNode>();
        public Dictionary<string, LightList<StyleConstant>> importedStyleConstants = new Dictionary<string, LightList<StyleConstant>>();
        
        public Dictionary<string, StyleConstant> constantsWithReferences = new Dictionary<string, StyleConstant>();
        public LightList<StyleConstant> constants = LightListPool<StyleConstant>.Get();
        public LightList<UIStyleGroup> importedGroups = LightListPool<UIStyleGroup>.Get();

        public void Clear() {
            constNodes.Clear();
            constants.Clear();
            importedGroups.Clear();
            constantsWithReferences.Clear();
        }

        public void Release() {
            LightListPool<StyleConstant>.Release(ref constants);
            LightListPool<UIStyleGroup>.Release(ref importedGroups);
        }
    }
}
