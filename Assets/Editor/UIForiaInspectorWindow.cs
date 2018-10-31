using Rendering;
using Src.Systems;
using UnityEditor;
using UnityEngine;

namespace Src.Editor {

    public class UIForiaInspectorWindow : EditorWindow {

        private UIElement selectedElement;
        private Vector2 scrollPosition;

        public void Update() {
            if (UIForiaHierarchyWindow.SelectedElement != selectedElement) {
                selectedElement = UIForiaHierarchyWindow.SelectedElement != null 
                    ? UIForiaHierarchyWindow.SelectedElement
                    : null;
                Repaint();
            }
        }

        private static readonly GUIRect s_GUIRect = new GUIRect();

        public void OnGUI() {
            if (selectedElement == null) {
                GUILayout.Label("Select an element in the UIForia Hierarchy Window");
                return;
            }

            s_GUIRect.SetRect(new Rect(0, 0, position.width - 20f, 2000f));

            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPosition, new Rect(0, 0, position.width - 20f, position.height + 100));
            LayoutResult layoutResult = selectedElement.layoutResult;

            EditorGUIUtility.wideMode = true;
            GUI.enabled = false;

            GUI.Label(s_GUIRect.GetFieldRect(), "Layout Result");
            s_GUIRect.GetFieldRect();

            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Local Position", layoutResult.localPosition);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Screen Position", layoutResult.ScreenPosition);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Scale", layoutResult.scale);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Content Offset", layoutResult.contentOffset);

            EditorGUI.FloatField(s_GUIRect.GetFieldRect(), "Rotation", layoutResult.rotation);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Allocated Size", layoutResult.allocatedSize);
            EditorGUI.Vector2Field(s_GUIRect.GetFieldRect(), "Actual Size", layoutResult.actualSize);

            s_GUIRect.GetFieldRect();
            EditorGUI.RectField(s_GUIRect.GetFieldRect(2), "Clip Rect", layoutResult.clipRect);
            EditorGUI.IntField(s_GUIRect.GetFieldRect(), "Render Layer", layoutResult.layer);
            EditorGUI.IntField(s_GUIRect.GetFieldRect(), "Z Index", layoutResult.zIndex);

            GUI.enabled = true;

            GUI.Label(s_GUIRect.GetFieldRect(), "Style Bindings");
            
            GUI.Label(s_GUIRect.GetFieldRect(), "Property Bindings");
            
            // ParsedTemplate.GetData(element.id);
            
            GUI.Label(s_GUIRect.GetFieldRect(), "Input Bindings");
    
            // Styles
            
            // for each style group
            
            // Overwrites
            
            // Has Scroll bar
            // Is Overflowing
            // Depth index
            // Style

            ComputedStyle style = selectedElement.ComputedStyle;

            GUI.EndScrollView();

//            GUILayout.BeginVertical();
//            {
//                GUILayout.Label("Layout Result");
//                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
//
//                EditorGUILayout.BeginHorizontal();
//                EditorGUILayout.Vector2Field("Local Position", layoutResult.localPosition);
//                EditorGUILayout.EndHorizontal();
//                EditorGUILayout.Vector2Field("Screen Position", layoutResult.ScreenPosition);
//                EditorGUILayout.Vector2Field("Scale", layoutResult.scale);
//                EditorGUILayout.Vector2Field("Content Offset", layoutResult.contentOffset);
//
//                EditorGUILayout.Space();
//
//                EditorGUILayout.FloatField("Rotation", layoutResult.rotation);
//                EditorGUILayout.Vector2Field("Allocated Size", layoutResult.allocatedSize);
//                EditorGUILayout.Vector2Field("Actual Size", layoutResult.actualSize);
//
//                EditorGUILayout.Space();
//                EditorGUILayout.RectField("Clip Rect", layoutResult.clipRect);
//                EditorGUILayout.IntField("Render Layer", layoutResult.layer);
//                EditorGUILayout.IntField("Z Index", layoutResult.zIndex);
//
//                GUILayout.EndScrollView();
//            }
//            GUILayout.EndVertical();
        }

    }

}