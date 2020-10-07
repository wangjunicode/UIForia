using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Elements;
using Debug = UnityEngine.Debug;

namespace UIForia.Compilers {

    [AttributeUsage(AttributeTargets.Method)]
    public class TemplateLocatorAttribute : Attribute {

        public readonly string name;

        public TemplateLocatorAttribute(string name) {
            this.name = name;
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class StyleLocatorAttribute : Attribute {

        public readonly string name;

        public StyleLocatorAttribute(string name) {
            this.name = name;
        }

    }

   

    public static class TypeScanner {

        internal static IList<Type> elementTypes;
        internal static IList<MethodInfo> changeHandlers;
        internal static Dictionary<string, Func<TemplateLookup, TemplateLocation>> templateLocators;
        internal static Dictionary<string, Func<StyleLookup, StyleLocation>> styleLocators;
        
        private static Stats stats;

        public static Stats Scan() {
            if (stats.totalScanTime != 0) return stats;
#if UNITY_EDITOR
            ScanFast();
#else
            throw new NotImplementedException("Cannot Scan for types outside of the editor");
#endif

            return stats;
        }

        private static void ScanFast() {
#if UNITY_EDITOR
            Stopwatch stopwatch = Stopwatch.StartNew();
            elementTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<UIElement>();
            changeHandlers = UnityEditor.TypeCache.GetMethodsWithAttribute<OnPropertyChanged>();

            UnityEditor.TypeCache.MethodCollection templateLocationFunctions = UnityEditor.TypeCache.GetMethodsWithAttribute<TemplateLocatorAttribute>();
            UnityEditor.TypeCache.MethodCollection styleLocationFunctions = UnityEditor.TypeCache.GetMethodsWithAttribute<StyleLocatorAttribute>();

            templateLocators = new Dictionary<string, Func<TemplateLookup, TemplateLocation>>();
            styleLocators = new Dictionary<string, Func<StyleLookup, StyleLocation>>();
            
            foreach (MethodInfo method in templateLocationFunctions) {
                Func<TemplateLookup, TemplateLocation> del = null;

                if (!method.IsStatic) { }

                if (!method.IsPublic) { }

                var parameters = method.GetParameters();
                
                if(parameters.Length != 1) {}

                if (parameters[0].ParameterType != typeof(TemplateLookup)) {
                    
                }
                
                if (method.ReturnType != typeof(TemplateLocation)) { }

                try {
                    del = (Func<TemplateLookup, TemplateLocation>) Delegate.CreateDelegate(typeof(Func<TemplateLookup, TemplateLocation>), method, true);
                }
                catch (Exception e) {

                    Debug.Log(e);
                }

                TemplateLocatorAttribute attr = method.GetCustomAttribute<TemplateLocatorAttribute>();

                if (templateLocators.TryGetValue(attr.name, out var _)) { }
                else {
                    templateLocators.Add(attr.name, del);
                }

            }

            foreach (MethodInfo method in styleLocationFunctions) {
                Func<StyleLookup, StyleLocation> del = null;
                if (!method.IsStatic) { }

                if (!method.IsPublic) { }

                if (method.ReturnType != typeof(string)) { }

                try {
                    del = (Func<StyleLookup, StyleLocation>) Delegate.CreateDelegate(typeof(Func<StyleLookup, StyleLocation>), method, true);
                }
                catch (Exception e) {
                    Debug.Log(e);
                }

                StyleLocatorAttribute attr = method.GetCustomAttribute<StyleLocatorAttribute>();

                if (styleLocators.TryGetValue(attr.name, out Func<StyleLookup, StyleLocation> _)) { }
                else {
                    styleLocators.Add(attr.name, del);
                }

            }

            stats.totalScanTime = stopwatch.Elapsed.TotalMilliseconds;
            stats.elementTypeCount = elementTypes.Count;
            stats.changeHandlerCount = changeHandlers.Count;

#endif

        }

        public struct Stats {

            public double totalScanTime;
            public int elementTypeCount;
            public int changeHandlerCount;

        }

    }

}