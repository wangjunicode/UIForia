using System.IO;
using System.Runtime.CompilerServices;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace UIForia.Editor {

    public static class WindowMenuItems {

        [MenuItem("Window/UIForia/UIForia Hierarchy")]
        private static void UIForiaHierarchy() {
            EditorWindow.GetWindow<UIForiaHierarchyWindow>("UIForia Hierarchy");
        }

        [MenuItem("Window/UIForia/UIForia Layout Hierarchy")]
        private static void UIForiaLayoutHierarchy() {
            EditorWindow.GetWindow<UIForiaLayoutHierarchyWindow>("UIForia Layout Hierarchy");
        }

        [MenuItem("Window/UIForia/UIForia Inspector")]
        private static void UIForiaInspector() {
            EditorWindow.GetWindow<UIForiaInspectorWindow>("UIForia Inspector");
        }

        [MenuItem("UIForia/Refresh UI Templates %g")]
        public static void Refresh() {
            if (UnityEngine.Application.isPlaying) {
                Application.RefreshAll();
            }
        }

        [MenuItem("UIForia/Dev/Generate Code Templates")]
        public static void GenerateInternalCodeTemplates() {
            string templatePath = Path.Combine(GetPath(), "..", "Generated_List_Template.cs");
            string outputPathRoot = Path.Combine(Application.GetSourceDirectory(), "Generated", "ListTypes");
            string contents = File.ReadAllText(templatePath);
            
            ListTemplateGenerator.Generate<FlexTrack>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<TextLayoutSymbol>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<TextLineInfo>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<TextSymbol>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<char>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<float>(outputPathRoot, contents, "float");
            ListTemplateGenerator.Generate<GridTrack>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<GridPlacement>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<float2>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<float3>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<float4>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<int>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<Color>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<Color32>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<BytePage>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<DrawInfo>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<VertexChannelDesc>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<UIForiaVertex>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<TextRenderRange>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<UIForiaGlyph>(outputPathRoot, contents);
            ListTemplateGenerator.Generate<TextMaterialInfo>(outputPathRoot, contents);
            
        }

        private class ListTemplateGenerator {

            public static void Generate<T>(string outputPath, string contents, string nameoverride = null) where T : struct {

                contents = contents.Replace("// #NAMESPACES#", "using " + typeof(T).Namespace + ";");
                contents = contents.Replace("public struct TTEMPLATE { }", "");
                contents = contents.Replace("TTEMPLATE", nameoverride ?? typeof(T).Name);

                File.WriteAllText(Path.Combine(outputPath, "List_" + (nameoverride ?? typeof(T).Name) + ".cs"), contents);
            }

        }

        public static string GetPath([CallerFilePath] string path = "") {
            return path;
        }

    }

}