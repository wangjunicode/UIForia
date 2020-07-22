using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Vector3 v0;
    public Vector3 v1;
    public Vector3 v2;
    public Vector3 v3;

    public Vector2 uv0;
    public Vector2 uv1;
    public Vector2 uv2;
    public Vector2 uv3;

    public int idx;
    
    // Update is called once per frame
    void Update() {
        Mesh mesh = GetComponent<TextMeshProUGUI>().mesh;
        var verts = mesh.vertices;
        if (verts.Length > 0 && idx + 4 < verts.Length) {
            v0 = verts[4 * idx + 0];
            v1 = verts[4 * idx + 1];
            v2 = verts[4 * idx + 2];
            v3 = verts[4 * idx + 3];
        }

        var uvs = mesh.uv;
        if (uvs.Length > 0) {
            uv0 = uvs[4 * idx + 0];
            uv1 = uvs[4 * idx + 1];
            uv2 = uvs[4 * idx + 2];
            uv3 = uvs[4 * idx + 3];
        }
    }
}
