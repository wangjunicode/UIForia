using System.Globalization;

namespace Demo {
    public static class BuildingTexts {
        public static string Brush(string id) {
            switch (id.ToUpperInvariant()) {
                case "CEILING":
                    return "Roof Tile";
                case "CORNER_IN_BVL":
                case "CORNER_IN_PRP":
                case "CORNER_IN_RND":
                    return "Inside Corner";
                case "CORNER_OUT_BVL":
                case "CORNER_OUT_PRP":
                case "CORNER_OUT_RND":
                    return "Outside Corner";
                case "DOOR_BVL":
                case "DOOR_PRP":
                case "DOOR_RND":
                    return "Door";
                case "FLOOR":
                    return "Floor Tile";
                case "WALL":
                case "WALL_IN_BVL":
                case "WALL_IN_PRP":
                case "WALL_IN_RND":
                    return "Inside Wall";
                case "WALL_OUT_BVL":
                case "WALL_OUT_PRP":
                case "WALL_OUT_RND":
                    return "Outside Wall";
                case "WINDOW_SMALL_BVL":
                case "WINDOW_SMALL_PRP":
                case "WINDOW_SMALL_RND":
                    return "Window";
                case "WINDOW_LARGE_BVL":
                case "WINDOW_LARGE_PRP":
                case "WINDOW_LARGE_RND":
                    return "Large Window";
                default:
                    return id.Replace("_", " ");
            }
        }
    }
}
