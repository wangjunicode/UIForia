using UIForia.Parsing.Style;
using UnityEditor;

namespace UIForia.Editor {

    public static class WindowMenuItems {

        [MenuItem("Window/UIForia Hierarchy")]
        private static void UIForiaHierarchy() {
            EditorWindow.GetWindow<UIForiaHierarchyWindow>("UIForia Hierarchy");
        }

        
        [MenuItem("Window/UIForia Inspector")]
        private static void UIForiaInspector() {
            EditorWindow.GetWindow<UIForiaInspectorWindow>("UIForia Inspector");
        }

        [MenuItem("UIForia/Refresh UI Templates %g")]
        public static void Refresh() {
            StyleParser.Reset();
            //TemplateParser.Reset();
            ResourceManager.Reset();
            ParsedTemplate.Reset(); // todo -- roll all parsers into one place
            
            Application.Game.Refresh();
            
        }
    }

}