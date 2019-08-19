using System;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class BetterRectPacker {

        public struct PackedRect {

            public int id;
            public float xMin;
            public float yMin;
            public float xMax;
            public float yMax;

        }

        public StructList<PackedRect> sortedRectList;
        public float totalWidth;
        public float totalHeight;
        public int checks;
        private static int s_IdGenerator;
        private Tile[,] tiles;
        private float tileWidth;
        private float tileHeight;
        
        public BetterRectPacker(float totalWidth, float totalHeight) {
            this.totalWidth = totalWidth;
            this.totalHeight = totalHeight;
            this.sortedRectList = new StructList<PackedRect>();
            
            this.tiles = new Tile[5,5];
            
            this.tileWidth = totalWidth / 5f;
            this.tileHeight = totalHeight / 5f;
            
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    tiles[i,j].x = tileWidth * i;     
                    tiles[i,j].y = tileHeight * j;
                }
            }
        }
        
        private int Gather(float xMin, float xMax, int start = 0) {
            int count = sortedRectList.size;
            PackedRect[] packedRects = sortedRectList.array;
            for (int i = start; i < count; i++) {
                ref PackedRect rect = ref packedRects[i];
                if (rect.xMin > xMax) {
                    return i;
                }
            }

            return count;
        }
        
        private struct Tile {

            public float x;
            public float y;
            public float occupiedArea;

        }
        
        public bool TryPackRect(float width, float height, out PackedRect retn) {
            retn = new PackedRect();

            if (width > totalWidth || height > totalHeight) {
                retn = default;
                return false;
            }

            int start = 0;
            int end = Gather(0, width);
            // if nothing has an xmax less than width 
            PackedRect[] packedRects = sortedRectList.array;
            float xMin = Single.MaxValue;
            bool safe = true;
            retn.xMin = 0;
            retn.yMin = 0;
            retn.xMax = width;
            retn.yMax = height;
            
            while (safe) {
                int intersectCount = 0;
                float yMax = float.MinValue;

                // for each tile
                // if tile contains or overlaps insert thing
                    // add to potential tile set
                // can overlap many tiles, thats fine
                // but for any check situation 
                // we can do a easy test to see if it is even possible to fit the thing in the area
                // if not, move it
                // if it might be, do the intersect
                
                for (int i = 0; i < sortedRectList.size; i++) {
                    ref PackedRect check = ref packedRects[i];

                   // if(check.xMax <= retn.xMin || check.yMax <= retn.yMin) continue;
//                 
//                   if (retn.xMax < check.xMin) {
//                       break;
//                   }
                   
                    bool intersects = !(retn.yMin >= check.yMax ||
                                        retn.yMax <= check.yMin ||
                                        retn.xMax <= check.xMin ||
                                        retn.xMin >= check.xMax);
                    checks++;

                    if (intersects) {
                        intersectCount++;
                        if (check.xMax < xMin) xMin = check.xMax;
                        if (check.yMax > yMax) yMax = check.yMax;
                    }
                }

                if (intersectCount == 0) {
                    retn.id = ++s_IdGenerator;

                    // todo -- make this smarter
                    for (int i = 0; i < sortedRectList.size; i++) {
                        if (retn.xMin >= packedRects[i].xMin) {
                            sortedRectList.Insert(i, retn);
                            return true;
                        }
                    }

                    sortedRectList.Add(retn);
                    return true;
                }

                retn.yMin = yMax;
                retn.yMax = retn.yMin + height;

                if (retn.yMax > totalHeight) {
                    retn.yMin = 0;
                    retn.yMax = height;
                    retn.xMin += (xMin - retn.xMin);
                    retn.xMax = retn.xMin + width;
                    xMin = float.MaxValue;
                    // todo -- find a better 'start' val

                    if (retn.xMax > totalWidth) {
                        retn = default;
                        return false;
                    }
                    
                    // end = Gather(retn.xMin, retn.xMax, end);
                }
            }

            return false;
        }

    }

}