using TMPro;
using UnityEngine;

namespace Debugger {

    public static class TexMeshProUtil {

        public static int FindNearestLine(TMP_TextInfo textInfo, Vector2 positionInRect) {
            float num1 = float.PositiveInfinity;
            int num2 = -1;

            for (int index = 0; index < textInfo.lineCount; ++index) {
                TMP_LineInfo tmpLineInfo = textInfo.lineInfo[index];
                float y1 = tmpLineInfo.ascender;
                float y2 = tmpLineInfo.descender;

                if (y1 > positionInRect.y && y2 < positionInRect.y) {
                    return index;
                }

                float num3 = Mathf.Min(Mathf.Abs(y1 - positionInRect.y), Mathf.Abs(y2 - positionInRect.y));
                if (num3 < num1) {
                    num1 = num3;
                    num2 = index;
                }
            }

            return num2;
        }

        public static int FindNearestCharacterOnLine(TMP_TextInfo textInfo, Vector2 position, int line, bool visibleOnly) {
            int firstCharacterIndex = textInfo.lineInfo[line].firstCharacterIndex;
            int lastCharacterIndex = textInfo.lineInfo[line].lastCharacterIndex;

            float num1 = float.PositiveInfinity;
            int num2 = lastCharacterIndex;

            for (int index = firstCharacterIndex; index < lastCharacterIndex; ++index) {
                TMP_CharacterInfo tmpCharacterInfo = textInfo.characterInfo[index];
                if (!visibleOnly || tmpCharacterInfo.isVisible) {
                    Vector3 vector3_1 = tmpCharacterInfo.bottomLeft;
                    Vector3 vector3_2 = new Vector3(tmpCharacterInfo.bottomLeft.x, tmpCharacterInfo.topRight.y, 0.0f);
                    Vector3 vector3_3 = tmpCharacterInfo.topRight;
                    Vector3 vector3_4 = new Vector3(tmpCharacterInfo.topRight.x, tmpCharacterInfo.bottomLeft.y, 0.0f);

                    Rect characterRect = new Rect(
                        new Vector2(tmpCharacterInfo.topLeft.x, tmpCharacterInfo.topLeft.y),
                        new Vector2(tmpCharacterInfo.bottomRight.x, tmpCharacterInfo.bottomRight.y)
                    );

                    if (characterRect.Contains(position)) {
                        num2 = index;
                        break;
                    }

                    float line1 = DistanceToLine(vector3_1, vector3_2, position);
                    float line2 = DistanceToLine(vector3_2, vector3_3, position);
                    float line3 = DistanceToLine(vector3_3, vector3_4, position);
                    float line4 = DistanceToLine(vector3_4, vector3_1, position);

                    float num3 = line1 >= line2 ? line2 : line1;
                    float num4 = num3 >= line3 ? line3 : num3;
                    float num5 = num4 >= line4 ? line4 : num4;

                    if (num1 > num5) {
                        num1 = num5;
                        num2 = index;
                    }
                }
            }

            return num2;
        }

        // todo convert to 2d
        public static float DistanceToLine(Vector3 a, Vector3 b, Vector3 point) {
            Vector3 vector3_1 = b - a;
            Vector3 vector3_2 = a - point;
            float num = Vector3.Dot(vector3_1, vector3_2);
            if ((double) num > 0.0)
                return Vector3.Dot(vector3_2, vector3_2);
            Vector3 vector3_3 = point - b;
            if ((double) Vector3.Dot(vector3_1, vector3_3) > 0.0)
                return Vector3.Dot(vector3_3, vector3_3);
            Vector3 vector3_4 = vector3_2 - vector3_1 * (num / Vector3.Dot(vector3_1, vector3_1));
            return Vector3.Dot(vector3_4, vector3_4);
        }

        public static int GetCursorIndexFromPosition(TMP_TextInfo textInfo, Vector2 position, out CaretPosition cursor) {
            int nearestLine = FindNearestLine(textInfo, position);
            int nearestCharacterOnLine = FindNearestCharacterOnLine(textInfo, position, nearestLine, false);

            if (textInfo.lineInfo[nearestLine].characterCount == 1) {
                cursor = CaretPosition.Left;
                return nearestCharacterOnLine;
            }

            TMP_CharacterInfo tmpCharacterInfo = textInfo.characterInfo[nearestCharacterOnLine];

            Vector3 vector3_1 = tmpCharacterInfo.bottomLeft;
            Vector3 vector3_2 = tmpCharacterInfo.topRight;

            if ((position.x - vector3_1.x) / (vector3_2.x - vector3_1.x) < 0.5) {
                cursor = CaretPosition.Left;
                return nearestCharacterOnLine;
            }

            cursor = CaretPosition.Right;
            return nearestCharacterOnLine;
        }

    }

}