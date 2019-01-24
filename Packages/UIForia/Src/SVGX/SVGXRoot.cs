using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UIForia;
using UIForia.Util;
using Unity.Collections;
using UnityEngine;

namespace SVGX {

    public class SVGXClipElement {

        public SVGXClipElement parentClipElement;
        public List<SVGXRenderElement> shapes;

    }

    public struct SVGXRenderElement {

        public List<Vector2> strokeAA;
        public List<Vector2> stroke;
        public List<Vector2> fill;
        public SVGXClipElement clip;
        public SVGXRenderElementType type;
        public SVGXStyle style;
        public SVGXMatrix transform;

    }

    public class SVGXRoot : MonoBehaviour {

        public Material paint;
        public Material stencil;
        public new Camera camera;
        private Mesh mesh;
        private Mesh lineMesh;

        private Mesh testClipArea;
        private Mesh blueRect;
        private Mesh greenRect;
        
        public Material clip;
        public Material stencilClear;

        public void Start() {
            testClipArea = SVGXGeometryGenerator.MakeRect(new Rect(0, 0, 200, 200), Color.red);
            blueRect = SVGXGeometryGenerator.MakeCutoutRect(new Rect(20, 0, 100, 100), Color.blue);
            greenRect = SVGXGeometryGenerator.MakeRect(new Rect(40, 40, 100, 100), Color.green);
        }

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            Vector3 origin = camera.transform.position + new Vector3(0, 0, 2);

            Matrix4x4 clipMat = Matrix4x4.TRS(origin + new Vector3(0, 0, 3), Quaternion.identity, Vector3.one);

            Graphics.DrawMesh(testClipArea, clipMat, clip, 0, camera, 0, null, false, false, false);

            Matrix4x4 blueMat = Matrix4x4.TRS(origin + new Vector3(0, 0, 1), Quaternion.identity, Vector3.one);
            Graphics.DrawMesh(blueRect, blueMat, stencil, 0, camera, 0, null, false, false, false);
            Graphics.DrawMesh(blueRect, blueMat, paint, 0, camera, 0, null, false, false, false);
            Graphics.DrawMesh(blueRect, blueMat, stencilClear, 0, camera, 0, null, false, false, false);

            // if not self intersection && no holes -> all good
            // otherwise we need to reset the fill bit in the stencil buffer
            // or cycle the fill bit but this requires unique materials or property blocks

            Matrix4x4 greenMatrix = Matrix4x4.TRS(origin + new Vector3(0, 0, 2), Quaternion.identity, Vector3.one);
            Graphics.DrawMesh(greenRect, greenMatrix, stencil, 0, camera, 0, null, false, false, false);
            Graphics.DrawMesh(greenRect, greenMatrix, paint, 0, camera, 0, null, false, false, false);
        }

    }
   
}
