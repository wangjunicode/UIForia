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
            if (UnityEngine.Application.isPlaying) {
                Application.RefreshAll();
            }
        }
        
        [MenuItem("UIForia/Pre Compile Templates %t")]
        public static void CompileTemplates() {
            // for now compiles all templates, probably don't want this in the future
//            Application.BuildTemplates(typeof());
        }
        
    }

}