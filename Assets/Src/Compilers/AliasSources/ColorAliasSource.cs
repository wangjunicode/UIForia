using UnityEngine;

namespace Src.Compilers.AliasSource {

    public class ColorAliasSource : IAliasSource {

        public object ResolveAlias(string alias, object data = null) {
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

    }

}