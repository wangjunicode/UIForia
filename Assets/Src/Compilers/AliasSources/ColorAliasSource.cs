using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Src.Compilers.AliasSource {

    public class ColorAliasSource : IAliasSource {

        public object ResolveAlias(string alias, object data = null) {
            Color color;
            if (ColorUtility.TryParseHtmlString(alias, out color)) {
                return color;
            }

            switch (alias) {
                case "black":
                case "Black":
                    return Color.black;
                case "blue":
                case "Blue":
                    return Color.blue;
                case "clear":
                case "Clear":
                    return Color.clear;
                case "cyan":
                case "Cyan":
                    return Color.cyan;
                case "gray":
                case "grey":
                case "Gray":
                case "Grey":
                    return Color.gray;
                case "green":
                case "Green":
                    return Color.green;
                case "magenta":
                case "Magenta":
                    return Color.magenta;
                case "red":
                case "Red":
                    return Color.red;
                case "white":
                case "White":
                    return Color.white;
                case "yellow":
                case "Yellow":
                    return Color.yellow;
                default: return null;
            }
        }

        public static MethodInfo ColorConstructor => typeof(ColorAliasSource).GetMethod(nameof(_ColorConstructor));
        public static MethodInfo ColorConstructorAlpha => typeof(ColorAliasSource).GetMethod(nameof(_ColorConstructorAlpha));
        
        [Pure]
        public static Color _ColorConstructor(int r, int g, int b) {
            return new Color32((byte)r, (byte)g, (byte)b, byte.MaxValue);
        }
        
        [Pure]
        public static Color _ColorConstructorAlpha(int r, int g, int b, int a) {
            return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }

    }

}