namespace UIForia.Util {

    public static class MathUtil {

        public static float PercentOfRange(float v, float bottom, float top) {
            float div = top - bottom;
            return div == 0 ? 0 : (v - bottom) / div;
        }

    }

}