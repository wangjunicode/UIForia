using TMPro;
using UnityEngine;

public static class TextMeshProUtil {

    public static int FindNearestLine(TMP_TextInfo textInfo, Vector2 point) {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        // lines have a gap between them, check for containment first
        // then track distances and return index of smallest distance
        // if no containment is found.
        for (int i = 0; i < textInfo.lineCount; i++) {
            TMP_LineInfo tmpLineInfo = textInfo.lineInfo[i];
            float y1 = -tmpLineInfo.ascender;
            float y2 = -tmpLineInfo.descender;

            if (point.y >= y1 && point.y <= y2) {
                return i;
            }
            
            float distToY1 = Mathf.Abs(point.y - y1);
            float distToY2 = Mathf.Abs(point.y - y2);
            if (distToY1 < closestDistance) {
                closestIndex = i;
                closestDistance = distToY1;
            }

            if (distToY2 < closestDistance) {
                closestIndex = i;
                closestDistance = distToY2;
            }
        }

        return closestIndex;
    }

    public static int FindNearestCharacterOnLine(TMP_TextInfo textInfo, Vector2 position, int line, bool visibleOnly) {
        int firstCharacterIndex = textInfo.lineInfo[line].firstCharacterIndex;
        int characterCount = textInfo.lineInfo[line].characterCount;

        float closestDistance = float.MaxValue;
        int closestIndex = firstCharacterIndex + characterCount - 1;

        for (int i = 0; i < characterCount; i++) {
            TMP_CharacterInfo tmpCharacterInfo = textInfo.characterInfo[firstCharacterIndex + i];
            if (visibleOnly && !tmpCharacterInfo.isVisible) {
                continue;
            }

            Vector2 topLeft = tmpCharacterInfo.topLeft;
            Vector2 topRight = tmpCharacterInfo.topRight;
            Vector2 bottomLeft = tmpCharacterInfo.bottomLeft;
            Vector2 bottomRight = tmpCharacterInfo.bottomRight;

            topLeft.y = -topLeft.y;
            topRight.y = -topRight.y;
            bottomLeft.y = -bottomLeft.y;
            bottomRight.y = -bottomRight.y;

            float minX = topLeft.x;
            float maxX = bottomRight.x;
            
            float minY = topLeft.y;
            float maxY = bottomRight.y;
            
            if (topLeft.x > bottomRight.x) {
                minX = bottomRight.x;
                maxX = topLeft.x;
            }

            if (topLeft.y > bottomRight.y) {
                minY = bottomRight.y;
                maxY = topLeft.y;
            }
            
            bool xContains = position.x >= minX && position.x <= maxX;
            bool yContains = position.y >= minY && position.y <= maxY;

            if (xContains && yContains) {
                return firstCharacterIndex + i;
            }           

            // todo maaaaybe this should be distance to line instead of distance to point
            float min = (position - topLeft).sqrMagnitude;
            float distSqrTopRight = (position - topRight).sqrMagnitude;
            float distSqrBottomLeft = (position - bottomLeft).sqrMagnitude;
            float distSqrBottomRight = (position - bottomRight).sqrMagnitude;

            if (distSqrTopRight < min) {
                min = distSqrTopRight;
            }

            if (distSqrBottomLeft < min) {
                min = distSqrBottomLeft;
            }

            if (distSqrBottomRight < min) {
                min = distSqrBottomRight;
            }

            if (min < closestDistance) {
                closestDistance = min;
                closestIndex = firstCharacterIndex + i;
            }

        }

        return closestIndex;
    }

    public static int GetCursorIndexFromPosition(TMP_TextInfo textInfo, Vector2 position, out CaretPosition cursor) {
        int nearestLine = FindNearestLine(textInfo, position);
        
        int closestLineIdx = FindNearestLine(textInfo, position);
        int closestCharIdx = FindNearestCharacterOnLine(textInfo, position, closestLineIdx, false);

        if (textInfo.lineInfo[nearestLine].characterCount == 1) {
            cursor = CaretPosition.Left;
            return closestCharIdx;
        }

        TMP_CharacterInfo tmpCharacterInfo = textInfo.characterInfo[closestCharIdx];
        
        Vector3 bottomLeft = tmpCharacterInfo.bottomLeft;
        Vector3 topRight = tmpCharacterInfo.topRight;

        if ((position.x - bottomLeft.x) / (topRight.x - bottomLeft.x) < 0.5) {
            cursor = CaretPosition.Left;
            return closestCharIdx;
        }

        cursor = CaretPosition.Right;
        return closestCharIdx;
    }

    public static int FindIntersectingWord(TMP_TextInfo textInfo, Vector3 position) {
        for (int i = 0; i < textInfo.wordCount; i++) {
            TMP_WordInfo wInfo = textInfo.wordInfo[i];

            bool isBeginRegion = false;

            Vector3 bl = Vector3.zero;
            Vector3 tl = Vector3.zero;
            Vector3 br = Vector3.zero;
            Vector3 tr = Vector3.zero;

            float maxAscender = -Mathf.Infinity;
            float minDescender = Mathf.Infinity;

            // Iterate through each character of the word
            for (int j = 0; j < wInfo.characterCount; j++) {
                int characterIndex = wInfo.firstCharacterIndex + j;
                TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterIndex];
                int currentLine = currentCharInfo.lineNumber;

                bool isCharacterVisible = currentCharInfo.isVisible;

                // Track maximum Ascender and minimum Descender for each word.
                maxAscender = Mathf.Max(maxAscender, -currentCharInfo.ascender);
                minDescender = Mathf.Min(minDescender, -currentCharInfo.descender);

                if (isBeginRegion == false && isCharacterVisible) {
                    isBeginRegion = true;

                    bl = new Vector3(currentCharInfo.bottomLeft.x, -currentCharInfo.descender, 0);
                    tl = new Vector3(currentCharInfo.bottomLeft.x, -currentCharInfo.ascender, 0);

                    // If Word is one character
                    if (wInfo.characterCount == 1) {
                        isBeginRegion = false;

                        br = new Vector3(currentCharInfo.topRight.x, -currentCharInfo.descender, 0);
                        tr = new Vector3(currentCharInfo.topRight.x, -currentCharInfo.ascender, 0);

                        // Transform coordinates to be relative to transform and account min descender and max ascender.
                        bl = new Vector3(bl.x, minDescender, 0);
                        tl = new Vector3(tl.x, maxAscender, 0);
                        tr = new Vector3(tr.x, maxAscender, 0);
                        br = new Vector3(br.x, minDescender, 0);

                        if (PointIntersectRectangle(position, bl, tl, tr, br)) {
                            return i;
                        }
                    }
                }

                // Last Character of Word
                if (isBeginRegion && j == wInfo.characterCount - 1) {
                    isBeginRegion = false;

                    br = new Vector3(currentCharInfo.topRight.x, -currentCharInfo.descender, 0);
                    tr = new Vector3(currentCharInfo.topRight.x, -currentCharInfo.ascender, 0);

                    bl = new Vector3(bl.x, minDescender, 0);
                    tl = new Vector3(tl.x, maxAscender, 0);
                    tr = new Vector3(tr.x, maxAscender, 0);
                    br = new Vector3(br.x, minDescender, 0);

                    if (PointIntersectRectangle(position, bl, tl, tr, br)) {
                        return i;
                    }
                }
                // If Word is split on more than one line.
                else if (isBeginRegion && currentLine != textInfo.characterInfo[characterIndex + 1].lineNumber) {
                    isBeginRegion = false;

                    br = new Vector3(currentCharInfo.topRight.x, -currentCharInfo.descender, 0);
                    tr = new Vector3(currentCharInfo.topRight.x, -currentCharInfo.ascender, 0);

                    // Transform coordinates to be relative to transform and account min descender and max ascender.
                    bl = new Vector3(bl.x, minDescender, 0);
                    tl = new Vector3(tl.x, maxAscender, 0);
                    tr = new Vector3(tr.x, maxAscender, 0);
                    br = new Vector3(br.x, minDescender, 0);

                    maxAscender = -Mathf.Infinity;
                    minDescender = Mathf.Infinity;

                    // Check for Intersection
                    if (PointIntersectRectangle(position, bl, tl, tr, br)) {
                        return i;
                    }
                }
            }
        }

        return -1;
    }
    
    public static int LineUpCharacterPosition(TMP_TextInfo textInfo, int originalPos, bool goToFirstChar) {
        if (originalPos >= textInfo.characterCount) {
            originalPos -= 1;
        }

        TMP_CharacterInfo originChar = textInfo.characterInfo[originalPos];
        int originLine = originChar.lineNumber;

        // We are on the first line return first character
        if (originLine - 1 < 0) {
            return goToFirstChar ? 0 : originalPos;
        }

        int endCharIdx = textInfo.lineInfo[originLine].firstCharacterIndex - 1;

        int closest = -1;
        float distance = TMP_Math.FLOAT_MAX;
        float range = 0;

        for (int i = textInfo.lineInfo[originLine - 1].firstCharacterIndex; i < endCharIdx; ++i) {
            TMP_CharacterInfo currentChar = textInfo.characterInfo[i];

            float d = originChar.origin - currentChar.origin;
            float r = d / (currentChar.xAdvance - currentChar.origin);

            if (r >= 0 && r <= 1) {
                return r < 0.5f ? i : i + 1;
            }

            d = Mathf.Abs(d);

            if (d < distance) {
                closest = i;
                distance = d;
                range = r;
            }
        }

        if (closest == -1) {
            return endCharIdx;
        }

        return (range < 0.5f) ? closest : closest + 1;
    }

    public static int LineDownCharacterPosition(TMP_TextInfo textInfo, int originalPos, bool goToLastChar) {
        if (originalPos >= textInfo.characterCount) {
            return textInfo.characterCount - 1;
        }

        TMP_CharacterInfo originChar = textInfo.characterInfo[originalPos];
        int originLine = originChar.lineNumber;

        //// We are on the last line return last character
        if (originLine + 1 >= textInfo.lineCount) {
            return goToLastChar ? textInfo.characterCount - 1 : originalPos;
        }

        // Need to determine end line for next line.
        int endCharIdx = textInfo.lineInfo[originLine + 1].lastCharacterIndex;

        int closest = -1;
        float distance = TMP_Math.FLOAT_MAX;
        float range = 0;

        for (int i = textInfo.lineInfo[originLine + 1].firstCharacterIndex; i < endCharIdx; ++i) {
            TMP_CharacterInfo currentChar = textInfo.characterInfo[i];

            float d = originChar.origin - currentChar.origin;
            float r = d / (currentChar.xAdvance - currentChar.origin);

            if (r >= 0 && r <= 1) {
                return (r < 0.5f) ? i : i + 1;
            }

            d = Mathf.Abs(d);

            if (d < distance) {
                closest = i;
                distance = d;
                range = r;
            }
        }

        if (closest == -1) return endCharIdx;

        return (range < 0.5f) ? closest : closest + 1;
    }
    
    public static int PageUpCharacterPosition(TMP_TextInfo textInfo, float viewportHeight, int originalPos, bool goToFirstChar) {
        if (originalPos >= textInfo.characterCount)
            originalPos -= 1;

        TMP_CharacterInfo originChar = textInfo.characterInfo[originalPos];
        int originLine = originChar.lineNumber;

        // We are on the first line return first character
        if (originLine - 1 < 0) {
            return goToFirstChar ? 0 : originalPos;
        }

        int newLine = originLine - 1;
        // Iterate through each subsequent line to find the first baseline that is not visible in the viewport.
        for (; newLine > 0; newLine--) {
            if (textInfo.lineInfo[newLine].baseline > textInfo.lineInfo[originLine].baseline + viewportHeight) {
                break;
            }
        }

        int endCharIdx = textInfo.lineInfo[newLine].lastCharacterIndex;

        int closest = -1;
        float distance = TMP_Math.FLOAT_MAX;
        float range = 0;

        for (int i = textInfo.lineInfo[newLine].firstCharacterIndex; i < endCharIdx; ++i) {
            TMP_CharacterInfo currentChar = textInfo.characterInfo[i];

            float d = originChar.origin - currentChar.origin;
            float r = d / (currentChar.xAdvance - currentChar.origin);

            if (r >= 0 && r <= 1) {
                return (r < 0.5f) ? i : i + 1;
            }

            d = Mathf.Abs(d);

            if (d < distance) {
                closest = i;
                distance = d;
                range = r;
            }
        }

        if (closest == -1) return endCharIdx;

        return (range < 0.5f) ? closest : closest + 1;
    }

     public static int PageDownCharacterPosition(TMP_TextInfo textInfo, float viewportHeight, int originalPos, bool goToLastChar) {
         if (originalPos >= textInfo.characterCount) {
             return textInfo.characterCount - 1;
         }

         TMP_CharacterInfo originChar = textInfo.characterInfo[originalPos];
        int originLine = originChar.lineNumber;

        // We are on the last line return last character
         if (originLine + 1 >= textInfo.lineCount) {
             return goToLastChar ? textInfo.characterCount - 1 : originalPos;
         }

         int newLine = originLine + 1;
        // Iterate through each subsequent line to find the first baseline that is not visible in the viewport.
        for (; newLine < textInfo.lineCount - 1; newLine++) {
            if (textInfo.lineInfo[newLine].baseline < textInfo.lineInfo[originLine].baseline - viewportHeight) {
                break;
            }
        }

        // Need to determine end line for next line.
        int endCharIdx = textInfo.lineInfo[newLine].lastCharacterIndex;

        int closest = -1;
        float distance = TMP_Math.FLOAT_MAX;
        float range = 0;

        for (int i = textInfo.lineInfo[newLine].firstCharacterIndex; i < endCharIdx; ++i) {
            TMP_CharacterInfo currentChar = textInfo.characterInfo[i];

            float d = originChar.origin - currentChar.origin;
            float r = d / (currentChar.xAdvance - currentChar.origin);

            if (r >= 0 && r <= 1) {
                return (r < 0.5f) ? i : i + 1;
            }

            d = Mathf.Abs(d);

            if (d < distance) {
                closest = i;
                distance = d;
                range = r;
            }
        }

        if (closest == -1) return endCharIdx;

         return (range < 0.5f) ? closest : closest + 1;
    }
    
    private static bool PointIntersectRectangle(Vector3 m, Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
        Vector3 ab = b - a;
        Vector3 am = m - a;
        Vector3 bc = c - b;
        Vector3 bm = m - b;

        float abamDot = Vector3.Dot(ab, am);
        float bcbmDot = Vector3.Dot(bc, bm);

        return 0 <= abamDot && abamDot <= Vector3.Dot(ab, ab) && 0 <= bcbmDot && bcbmDot <= Vector3.Dot(bc, bc);
    }

}