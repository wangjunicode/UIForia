using System.Collections.Generic;
using UnityEngine;

namespace SVGX {

    public static class SVGXGeometryGenerator {

        
        public static Mesh FillLineMesh(List<Vector2> segments) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector4> prevNext = new List<Vector4>();
            List<Vector4> uvNormal = new List<Vector4>();
            List<Vector4> flags = new List<Vector4>();
            List<Color32> colors = new List<Color32>();

            Mesh retn = new Mesh();

            // vertex format:
            // 4 vertices per 2 segments
            // 0 --- 1|4 --- 5|08 ---- 09
            // |      |       |        |
            // 2 --- 3|6 --- 7|10 ---- 11

            Vector2 dir = (segments[1] - segments[0]).normalized;
            Vector2 prev = segments[0] - dir;
            Vector2 curr = segments[0];
            Vector2 next = segments[1];
            Vector2 far = segments.Count == 2 ? segments[1] + (segments[1] - segments[0]).normalized : segments[2];

            vertices.Add(curr);
            vertices.Add(next);
            vertices.Add(curr);
            vertices.Add(next);

            flags.Add(new Vector4(1, -1, 0, 0));
            flags.Add(new Vector4(1, 1, 0, 0));
            flags.Add(new Vector4(-1, -1, 0, 0));
            flags.Add(new Vector4(-1, 1, 0, 0));

            prevNext.Add(new Vector4(prev.x, prev.y, next.x, next.y));
            prevNext.Add(new Vector4(curr.x, curr.y, far.x, far.y));
            prevNext.Add(new Vector4(prev.x, prev.y, next.x, next.y));
            prevNext.Add(new Vector4(curr.x, curr.y, far.x, far.y));

            colors.Add(Color.red);
            colors.Add(Color.red);
            colors.Add(Color.red);
            colors.Add(Color.red);

            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(2);
            triangles.Add(1);
            triangles.Add(3);

            int triIdx = 4;

            for (int i = 1; i < segments.Count - 2; i++) {
                prev = segments[i - 1];
                curr = segments[i];
                next = segments[i + 1];
                far = segments[i + 2];

                vertices.Add(curr);
                vertices.Add(next);
                vertices.Add(curr);
                vertices.Add(next);

                prevNext.Add(new Vector4(prev.x, prev.y, next.x, next.y));
                prevNext.Add(new Vector4(curr.x, curr.y, far.x, far.y));
                prevNext.Add(new Vector4(prev.x, prev.y, next.x, next.y));
                prevNext.Add(new Vector4(curr.x, curr.y, far.x, far.y));

                flags.Add(new Vector4(1, -1, 0, 0));
                flags.Add(new Vector4(1, 1, 0, 0));
                flags.Add(new Vector4(-1, -1, 0, 0));
                flags.Add(new Vector4(-1, 1, 0, 0));

                colors.Add(Color.yellow);
                colors.Add(Color.yellow);
                colors.Add(Color.yellow);
                colors.Add(Color.yellow);

                triangles.Add(triIdx + 0);
                triangles.Add(triIdx + 1);
                triangles.Add(triIdx + 2);
                triangles.Add(triIdx + 2);
                triangles.Add(triIdx + 1);
                triangles.Add(triIdx + 3);
                triIdx += 4;
            }

            if (segments.Count > 2) {
                int currIdx = segments.Count - 2;
                prev = segments[currIdx - 1];
                curr = segments[currIdx];
                next = segments[currIdx + 1];
                far = next + (next - curr);

                vertices.Add(curr);
                vertices.Add(next);
                vertices.Add(curr);
                vertices.Add(next);

                prevNext.Add(new Vector4(prev.x, prev.y, next.x, next.y));
                prevNext.Add(new Vector4(curr.x, curr.y, far.x, far.y));
                prevNext.Add(new Vector4(prev.x, prev.y, next.x, next.y));
                prevNext.Add(new Vector4(curr.x, curr.y, far.x, far.y));

                flags.Add(new Vector4(1, -1, 0, 0));
                flags.Add(new Vector4(1, 1, 0, 0));
                flags.Add(new Vector4(-1, -1, 0, 0));
                flags.Add(new Vector4(-1, 1, 0, 0));

                triangles.Add(triIdx + 0);
                triangles.Add(triIdx + 1);
                triangles.Add(triIdx + 2);
                triangles.Add(triIdx + 2);
                triangles.Add(triIdx + 1);
                triangles.Add(triIdx + 3);

                colors.Add(Color.green);
                colors.Add(Color.green);
                colors.Add(Color.green);
                colors.Add(Color.green);
            }

            retn.SetVertices(vertices);
            retn.SetTriangles(triangles, 0);
            retn.SetUVs(0, uvNormal);
            retn.SetUVs(1, prevNext);
            retn.SetUVs(2, flags);
            retn.SetColors(colors);
            return retn;
        }

        public static Mesh MakeRect(Rect rect, Color color) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colors = new List<Color>();

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            vertices.Add(new Vector3(rect.x, rect.y, 0));
            vertices.Add(new Vector3(rect.x + rect.width, rect.y, 0));
            vertices.Add(new Vector3(rect.x + rect.width, rect.y + rect.height, 0));
            vertices.Add(new Vector3(rect.x, rect.y + rect.height, 0));

            for (int i = 1; i < vertices.Count - 1; i++) {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            Mesh retn = new Mesh();
            retn.SetVertices(vertices);
            retn.SetColors(colors);
            retn.SetTriangles(triangles, 0);
            return retn;
        }

        public static Mesh MakeCutoutRect(Rect rect, Color color) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colors = new List<Color>();

            vertices.Add(new Vector3(rect.x, rect.y, 0));
            vertices.Add(new Vector3(rect.x + rect.width, rect.y, 0));
            vertices.Add(new Vector3(rect.x + rect.width, rect.y + rect.height, 0));
            vertices.Add(new Vector3(rect.x, rect.y + rect.height, 0));
            vertices.Add(new Vector3(rect.x + (rect.width * 0.5f), rect.y + (rect.height * 0.5f), 0));

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            for (int i = 1; i < vertices.Count - 1; i++) {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            Mesh retn = new Mesh();
            retn.SetVertices(vertices);
            retn.SetColors(colors);
            retn.SetTriangles(triangles, 0);
            return retn;
        }

    }

}