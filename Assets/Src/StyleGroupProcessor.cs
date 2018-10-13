using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Rendering;
using Src;
using Src.Rendering;
using Src.Util;

public static class StyleGroupProcessor {

    private static readonly Dictionary<string, List<ValueTuple<string, UIBaseStyleGroup>>> s_StyleGroupMap;

    static StyleGroupProcessor() {
        s_StyleGroupMap = new Dictionary<string, List<ValueTuple<string, UIBaseStyleGroup>>>();
    }

    public static UIBaseStyleGroup ResolveStyle(string classPath, string styleName) {
        List<ValueTuple<string, UIBaseStyleGroup>> groups;

        if (s_StyleGroupMap.TryGetValue(classPath, out groups)) {
            for (int i = 0; i < groups.Count; i++) {
                if (groups[i].Item1 == styleName) {
                    return groups[i].Item2;
                }
            }
        }

        Type styleType = TypeProcessor.GetRuntimeType(Path.GetFileNameWithoutExtension(classPath));

        if (styleType == null) return null;
        
        groups = ListPool<ValueTuple<string, UIBaseStyleGroup>>.Get();

        MethodInfo[] methods = styleType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        for (int i = 0; i < methods.Length; i++) {
            MethodInfo methodInfo = methods[i];

            ExportStyleAttribute attr = (ExportStyleAttribute) methodInfo.GetCustomAttribute(typeof(ExportStyleAttribute));

            if (attr == null) continue;

            if (!methodInfo.IsStatic) {
                throw new Exception($"Methods annotated with {nameof(ExportStyleAttribute)} must be static");
            }

            if (methods[i].GetParameters().Length != 0) {
                throw new Exception($"Methods annotated with {nameof(ExportStyleAttribute)} must not accept parameters");
            }

            if (methodInfo.ReturnType == typeof(UIStyle)) {
                UIBaseStyleGroup group = new UIBaseStyleGroup();
                group.normal = (UIStyle) methodInfo.Invoke(null, null);
                groups.Add(ValueTuple.Create(attr.name, group));
            }
            else if (methodInfo.ReturnType == typeof(UIBaseStyleGroup)) {
                groups.Add(ValueTuple.Create(attr.name, (UIBaseStyleGroup) methodInfo.Invoke(null, null)));
            }
            else {
                throw new Exception($"Methods annotated with {nameof(ExportStyleAttribute)} must return {nameof(UIStyle)} or {nameof(UIBaseStyleGroup)}");
            }
        }

        if (groups.Count == 0) {
            ListPool<ValueTuple<string, UIBaseStyleGroup>>.Release(ref groups);
            s_StyleGroupMap[classPath] = null;
        }
        else {
            s_StyleGroupMap[classPath] = groups;
            for (int i = 0; i < groups.Count; i++) {
                if (groups[i].Item1 == styleName) {
                    return groups[i].Item2;
                }
            }
        }

        return null;
    }

}