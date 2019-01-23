using System.Collections.Generic;
using UnityEngine;

namespace SVGX {

    public class SVGXRoot : MonoBehaviour {

        public Material paint;
        public Material stencil;
        public Material lineMaterial;
        public Camera camera;
        private Mesh mesh;
        private Mesh lineMesh;

        public void Start() {
            mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

//            vertices.Add(new Vector3(100, -10f));
//
//            vertices.Add(new Vector3(100f, -10f));
//            vertices.Add(new Vector3(40f, -198f));
//            vertices.Add(new Vector3(190f, -78f));
//            vertices.Add(new Vector3(10f, -78f));
//            vertices.Add(new Vector3(160f, -198f));
//            vertices.Add(new Vector3(100f, -10f));
//
//            for (int i = 1; i < vertices.Count - 1; i++) {
//                triangles.Add(0);
//                triangles.Add(i);
//                triangles.Add(i + 1);
//            }


//            mesh.SetVertices(vertices);
//            mesh.SetTriangles(triangles, 0);

            List<Vector2> segments = new List<Vector2>();

//            segments.Add(new Vector2(200f, -200f));
//            segments.Add(new Vector2(300f, -300f));
//            segments.Add(new Vector2(400f, -300f));
//            segments.Add(new Vector2(400f, -200f));

//            lineMesh = FillLineMesh(segments);
            SVGXPathElement path = new SVGXPathElement(new [] {
                PathCommand.MoveTo(100, -250),
                PathCommand.QuadraticCurveTo(250, -100, 400, -250),
            });

            List<Vector2> segs = path.Flatten()[0];
//            segs.Insert(0, segs[0]);
//            segs.Add(segs[0]);
//
//            for (int i = 0; i < segs.Count; i++) {
//                vertices.Add(new Vector3(segs[i].x, segs[i].y));
//            }
//            
//            for (int i = 1; i < vertices.Count - 1; i++) {
//                triangles.Add(0);
//                triangles.Add(i);
//                triangles.Add(i + 1);
//            }
//
//            mesh.SetVertices(vertices);
//            mesh.SetTriangles(triangles, 0);
            
            lineMesh = FillLineMesh(segs);
        }

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

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;
            Matrix4x4 matrix = Matrix4x4.TRS(camera.transform.position + new Vector3(0, 0, 2), Quaternion.identity, Vector3.one);

            Graphics.DrawMesh(lineMesh, matrix, lineMaterial, 0, camera, 0, null, false, false, false);

//            Graphics.DrawMesh(mesh, matrix, stencil, 0, camera, 0, null, false, false, false);
//            Graphics.DrawMesh(mesh, matrix, paint, 0, camera, 0, null, false, false, false);
        }

    }

}