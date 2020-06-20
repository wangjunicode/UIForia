using UnityEngine;

namespace UIForia.Graphics {

    public struct HSLColor {

        public float h;
        public float s;
        public float l;

        public static HSLColor FromRGB(byte R, byte G, byte B) {
            float _R = (R / 255f);
            float _G = (G / 255f);
            float _B = (B / 255f);

            float _Min = Mathf.Min(Mathf.Min(_R, _G), _B);
            float _Max = Mathf.Max(Mathf.Max(_R, _G), _B);
            float _Delta = _Max - _Min;

            float H = 0;
            float S = 0;
            float L = (float) ((_Max + _Min) / 2.0f);

            if (_Delta != 0) {
                if (L < 0.5f) {
                    S = _Delta / (_Max + _Min);
                }
                else {
                    S = _Delta / (2.0f - _Max - _Min);
                }

                if (_R == _Max) {
                    H = (_G - _B) / _Delta;
                }
                else if (_G == _Max) {
                    H = 2f + (_B - _R) / _Delta;
                }
                else if (_B == _Max) {
                    H = 4f + (_R - _G) / _Delta;
                }
            }

            return new HSLColor() {
                h = H,
                s = S,
                l = L
            };
        }

        public Color32 ToRGB(byte a = 255) {
            byte r, g, b;
            if (s == 0) {
                r = (byte) Mathf.Round(l * 255f);
                g = (byte) Mathf.Round(l * 255f);
                b = (byte) Mathf.Round(l * 255f);
            }
            else {
                float t2;
                float th = h / 6.0f;

                if (l < 0.5f) {
                    t2 = l * (1f + s);
                }
                else {
                    t2 = (l + s) - (l * s);
                }

                float t1 = 2f * l - t2;

                float tr, tg, tb;
                tr = th + (1.0f / 3.0f);
                tg = th;
                tb = th - (1.0f / 3.0f);

                tr = ColorCalc(tr, t1, t2);
                tg = ColorCalc(tg, t1, t2);
                tb = ColorCalc(tb, t1, t2);
                r = (byte) Mathf.Round(tr * 255f);
                g = (byte) Mathf.Round(tg * 255f);
                b = (byte) Mathf.Round(tb * 255f);
            }

            return new Color32(r, g, b, a);
        }

        private static float ColorCalc(float c, float t1, float t2) {

            if (c < 0) c += 1f;
            if (c > 1) c -= 1f;
            if (6.0f * c < 1.0f) return t1 + (t2 - t1) * 6.0f * c;
            if (2.0f * c < 1.0f) return t2;
            if (3.0f * c < 2.0f) return t1 + (t2 - t1) * (2.0f / 3.0f - c) * 6.0f;
            return t1;
        }

    }

}